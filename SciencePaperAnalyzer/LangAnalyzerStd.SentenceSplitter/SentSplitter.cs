using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

using LangAnalyzerStd.Core;
using LangAnalyzerStd.Urls;

namespace LangAnalyzerStd.SentenceSplitter
{
    using SentCharType = SentSplitterModel.SentCharType;

    public sealed class Sentence
    {
        private const int DEFAULT_URLS_LIST_CAPACITY = 4;

        internal Sentence() : this(0) { }
        internal Sentence(int _startIndex)
        {
            StartIndex = _startIndex;
            Length = 0;
        }

        internal Sentence(int _startIndex, int _length)
        {
            StartIndex = _startIndex;
            Length = _length;
        }

        public void Set4ModelBuilder(int _startIndex, int _length, List<Url> _urls)
        {
            StartIndex = _startIndex;
            Length = _length;
            Urls = _urls;
        }

        public static Sentence CreateEmpty()
        {
            return new Sentence();
        }

        public int StartIndex
        {
            get;
            private set;
        }
        public int Length
        {
            get;
            private set;
        }
        public List<Url> Urls
        {
            get;
            private set;
        }

        internal void SetLength(int _length)
        {
            Length = _length;
        }

        internal void AppendUrl(Url url)
        {
            if (Urls == null)
            {
                Urls = new List<Url>(DEFAULT_URLS_LIST_CAPACITY);
            }
            Urls.Add(url);
        }
        internal void SetAsFirst()
        {
            StartIndex = 0;
            Length = 0;
            Urls = null;
        }
        internal void Reset(int _startIndex)
        {
            StartIndex = _startIndex;
            Length = 0;
            Urls = null;
        }

        public string GetValue(string originalText)
        {
            return originalText.Substring(StartIndex, Length);
        }
        public Sentence CreateCopy()
        {
            var sent = new Sentence(StartIndex, Length);
            if (Urls != null)
            {
                sent.Urls = Urls.ToList();
            }
            return sent;
        }

        public override string ToString()
        {
            if (Urls != null)
                return $"[{StartIndex}:{Length}], url's: {Urls.Count}";
            return $"[{StartIndex}:{Length}]";
        }
    }

    unsafe public struct SentenceStruct
    {
        public char* startPtr;
        public int length;
        public List<UrlStruct> urls;
    }

    unsafe public sealed class SentSplitter : IDisposable
    {
        private class DotArea
        {
            public SsWord _headWord;   //head-word (in vicinity of dot)
            private SsWord _endWord;    //current end-word 
            public SsWord _leftWord;   //left-word  (from dot)
            public SsWord _rightWord;  //right-word (from dot)
            public int _wordsCount; //total count of words

            public bool HasWords
            {
                get { return _headWord != null; }
            }

            public void InsertToHead(SsWord word)
            {
                if (_headWord == null)
                {
                    _headWord = word;
                    _endWord = word;
                }
                else
                {
                    word.next = _headWord;
                    _headWord.prev = word;
                    _headWord = word;
                }
                _wordsCount++;
            }

            public void Add(SsWord word)
            {
                if (_headWord == null)
                {
                    _headWord = word;
                    _endWord = word;
                }
                else
                {
                    _endWord.next = word;
                    word.prev = _endWord;
                    _endWord = word;
                }
                _wordsCount++;
            }

            public void FixupLeftWord()
            {
                _leftWord = _endWord;
            }

            public void FixupRightWord()
            {
                if (_leftWord != null)
                {
                    _rightWord = _leftWord.next;
                }
                else
                {
                    _rightWord = _headWord;
                }
            }

            public bool HasWordAfterLeftWord
            {
                get { return _leftWord != null && _leftWord.next != null; }
            }

            public bool HasLeftWord
            {
                get { return _leftWord != null; }
            }

            public bool HasRightWord
            {
                get { return _rightWord != null; }
            }

            public void Reset()
            {
                _headWord = null;
                _endWord = null;
                _leftWord = null;
                _rightWord = null;
                _wordsCount = 0;
            }
        }

        #region private fields
        private const int DEFAULT_LIST_CAPACITY = 100;
        private const int NGRAM_MIN_LENGTH_TO_LEFT = 3;
        private static readonly char* MAX_PTR = (char*)(0xffffffffFFFFFFFF);
        private static readonly char* MIN_PTR = (char*)(0x0);
        private readonly SentSplitterModel _model;         //model 
        private bool _splitBySmiles; //split sent's by smile's
        private readonly int _NgramMaxLengthToLeft;  //max ngram's length to left
        private readonly int _NgramMaxLengthToRight; //max ngram's length to right
        private readonly DotArea _dotArea;   //vicinity of dot data
        private readonly UrlDetector _urlDetector;   //url-detecor
        private readonly List<Sentence> _sentences;         //list of sent-class'es
        private readonly StringBuilder _stringBuilder; //buffer string builder
        private readonly char[] _buffer;    //buffer for smile's & etc.
        private readonly GCHandle _bufferGCHandle;
        private char* _bufferPtrBase; //pointer to buffer
        private readonly CharType* _CTM;  //xlat.CHARTYPE_MAP
        private readonly char* _UIM;  //xlat.UPPER_INVARIANT_MAP
        private readonly SentCharType* _SCTM; //_Model.SENTCHARTYPE_MAP
        private Sentence _Sent; //current open sent
        private char* _BASE; //start pointer into text
        private char* _ptr;  //current pointer into text
        private int _openBrakets; //open bracket's count (on the left)
        private int _openQuotas; //open twin quota's count (on the left)        
        private bool _openQuotaDoubleSided; //open unpaired quota's (on the left)
        private List<UrlStruct> _urls;
        private int _urlIndex;
        private char* _endUrlPtr;
        private ProcessSentCallbackDelegate _outerProcessSentCallback_Delegate;
        #endregion

        public bool SplitBySmiles
        {
            get { return _splitBySmiles; }
            set { _splitBySmiles = value; }
        }

        public List<SentenceStruct> Sentstructs { get; }

        #region ctor dtor dispose
        public SentSplitter(SentSplitterConfig config)
        {
            _model = config.Model;
            _splitBySmiles = config.SplitBySmiles;
            _urlDetector = new UrlDetector(config.UrlDetectorConfig);

            _sentences = new List<Sentence>(DEFAULT_LIST_CAPACITY);
            _stringBuilder = new StringBuilder();
            _Sent = new Sentence();
            Sentstructs = new List<SentenceStruct>(DEFAULT_LIST_CAPACITY);

            _NgramMaxLengthToLeft = NGRAM_MIN_LENGTH_TO_LEFT;
            _NgramMaxLengthToRight = _model.GetNgramMaxLength() - 1;
            _dotArea = new DotArea();

            _CTM = XlatUnsafe.Inst._CHARTYPE_MAP;
            _UIM = XlatUnsafe.Inst._UPPER_INVARIANT_MAP;
            _SCTM = _model.SENTCHARTYPE_MAP;

            _buffer = new char[_model.GetValuesMaxLength() + 1];
            _bufferGCHandle = GCHandle.Alloc(_buffer, GCHandleType.Pinned);
            _bufferPtrBase = (char*)_bufferGCHandle.AddrOfPinnedObject().ToPointer();
        }

        ~SentSplitter()
        {
            DisposeNativeResources();
        }

        public void Dispose()
        {
            DisposeNativeResources();

            GC.SuppressFinalize(this);
        }

        private void DisposeNativeResources()
        {
            if (_bufferPtrBase != null)
            {
                _bufferGCHandle.Free();
                _bufferPtrBase = null;
            }
        }
        #endregion

        public delegate void ProcessSentCallbackDelegate(Sentence sent);

        public void AllocateSents(string text, ProcessSentCallbackDelegate processSentCallback)
        {
            _outerProcessSentCallback_Delegate = processSentCallback;

            _Sent.SetAsFirst();
            _openBrakets = 0;
            _openQuotas = 0;
            _openQuotaDoubleSided = false;

            fixed (char* _base = text)
            {
                _BASE = _base;

                _urls = _urlDetector.AllocateUrls(_base);
                _urlIndex = 0;
                _endUrlPtr = MIN_PTR;
                var startUrlPtr = (_urlIndex < _urls.Count) ? _urls[_urlIndex].startPtr : MAX_PTR;

                for (_ptr = _BASE; ;)
                {
                    if (startUrlPtr <= _ptr)
                    {
                        var url = _urls[_urlIndex];
                        _Sent.AppendUrl(Url.ToUrl(url, _BASE));
                        _ptr = _endUrlPtr = startUrlPtr + url.length;
                        _urlIndex++;
                        startUrlPtr = (_urlIndex < _urls.Count) ? _urls[_urlIndex].startPtr : MAX_PTR;
                        continue;
                    }

                    var ch = *_ptr;
                    if (ch == '\0')
                    {
                        break;
                    }

                    var sct = *(_SCTM + ch);
                    if (sct == SentCharType.Unconditional)
                    {
                        _ptr++;
                        SetSentAndCallback();
                        continue;
                    }

                    var ct = *(_CTM + ch);
                    switch ((ct & CharType.IsQuoteOrBracket))
                    {
                        case CharType.IsQuote:
                        case CharType.IsBracket:
                            if ((ct & CharType.IsBracketLeft) == CharType.IsBracketLeft)
                                _openBrakets++;
                            else
                            if ((ct & CharType.IsBracketRight) == CharType.IsBracketRight)
                                _openBrakets--;
                            else
                            {
                                if ((ct & CharType.IsQuoteLeft) == CharType.IsQuoteLeft)
                                {
                                    if ((ct & CharType.IsQuoteDoubleSided) == CharType.IsQuoteDoubleSided)
                                        _openQuotaDoubleSided = !_openQuotaDoubleSided;
                                    else
                                        _openQuotas++;
                                }
                                else
                                if ((ct & CharType.IsQuoteRight) == CharType.IsQuoteRight)
                                {
                                    if ((ct & CharType.IsQuoteDoubleSided) == CharType.IsQuoteDoubleSided)
                                        _openQuotaDoubleSided = !_openQuotaDoubleSided;
                                    else
                                        _openQuotas--;
                                }
                            }
                            break;
                    }


                    if ((sct & SentCharType.ExcludeInBracketAndQuote) == SentCharType.ExcludeInBracketAndQuote)
                    {
                        if (IsEndOfSentTDQMEP(sct))
                        {
                            SkipFollowPunctuation();
                            SetSentAndCallback();
                            continue;
                        }
                        _ptr++;
                        continue;
                    }

                    if (_splitBySmiles && (sct & SentCharType.SmileBegin) == SentCharType.SmileBegin)
                    {
                        var smile_length = TryGetSmileLength();
                        if (0 < smile_length)
                        {
                            _ptr += smile_length;
                            SetSentAndCallback();
                            continue;
                        }
                    }

                    if ((sct & SentCharType.Dot) == SentCharType.Dot)
                    {
                        if (IsPreviousAndNextCharDigit())
                        {
                            _ptr++;
                            continue;
                        }

                        if (!TokenizeDotArea())
                        {
                            _ptr++;
                            continue;
                        }

                        var offset = TryBeforeProperOrNumberBeforeNoProper();
                        if (offset.HasValue)
                        {
                            if (offset.Value < 0)
                            {
                                _ptr -= offset.Value;
                            }
                            else
                            {
                                _ptr += offset.Value;
                                SetSentAndCallback();
                            }
                            continue;
                        }

                        offset = TryGetThreeDotsLength();
                        if (offset.HasValue)
                        {
                            if (offset.Value < 0)
                            {
                                _ptr -= offset.Value;
                            }
                            else
                            {
                                _ptr += offset.Value;
                                SetSentAndCallback();
                            }
                            continue;
                        }


                        offset = TryGetSingleUpperChar();
                        if (offset.HasValue)
                        {
                            if (offset.Value < 0)
                            {
                                _ptr -= offset.Value;
                            }
                            else
                            {
                                _ptr += offset.Value;
                                SetSentAndCallback();
                            }
                            continue;
                        }

                        if (IsInterjection())
                        {
                            _ptr++;
                            SetSentAndCallback();
                            continue;
                        }

                        var len = TryGetFileExtensionLength();
                        if (0 < len)
                        {
                            _ptr += len;
                            continue;
                        }

                        len = TryGetYandexCombinationsLength();
                        if (0 < len)
                        {
                            _ptr += len;
                            continue;
                        }

                        offset = TryOtherSituation();
                        if (offset.HasValue)
                        {
                            if (offset.Value < 0)
                            {
                                _ptr -= offset.Value;
                            }
                            else
                            {
                                _ptr += offset.Value;
                                SetSentAndCallback();
                            }
                            continue;
                        }

                        len = TryEndOfQuotingLength();
                        if (0 < len)
                        {
                            _ptr += len;
                            continue;
                        }

                        len = TryListLength();
                        if (0 < len)
                        {
                            _ptr += len;
                            continue;
                        }

                        _ptr++;
                        SetSentAndCallback();
                        continue;
                    }

                    _ptr++;
                }

                SetLastSentAndCallback(text.Length);
            }

            _outerProcessSentCallback_Delegate = null;
        }
        public List<Sentence> AllocateSents(string text, bool splitBySmiles)
        {
            _splitBySmiles = splitBySmiles;

            return AllocateSents(text);
        }
        public List<Sentence> AllocateSents(string text)
        {
            _sentences.Clear();
            _Sent.SetAsFirst();
            _openBrakets = 0;
            _openQuotas = 0;
            _openQuotaDoubleSided = false;

            fixed (char* _base = text)
            {
                _BASE = _base;

                _urls = _urlDetector.AllocateUrls(_base);
                _urlIndex = 0;
                _endUrlPtr = MIN_PTR;
                var startUrlPtr = (_urlIndex < _urls.Count) ? _urls[_urlIndex].startPtr : MAX_PTR;

                for (_ptr = _BASE; ;)
                {
                    if (startUrlPtr <= _ptr)
                    {
                        var url = _urls[_urlIndex];
                        _Sent.AppendUrl(Url.ToUrl(url, _BASE));
                        _ptr = _endUrlPtr = startUrlPtr + url.length;
                        _urlIndex++;
                        startUrlPtr = (_urlIndex < _urls.Count) ? _urls[_urlIndex].startPtr : MAX_PTR;
                        continue;
                    }

                    var ch = *_ptr;
                    if (ch == '\0')
                    {
                        break;
                    }

                    var sct = *(_SCTM + ch);
                    if (sct == SentCharType.Unconditional)
                    {
                        _ptr++;
                        CreateSentAndPutToList();
                        continue;
                    }

                    var ct = *(_CTM + ch);
                    switch ((ct & CharType.IsQuoteOrBracket))
                    {
                        case CharType.IsQuote:
                        case CharType.IsBracket:
                            if ((ct & CharType.IsBracketLeft) == CharType.IsBracketLeft)
                                _openBrakets++;
                            else
                            if ((ct & CharType.IsBracketRight) == CharType.IsBracketRight)
                                _openBrakets--;
                            else
                            {
                                if ((ct & CharType.IsQuoteLeft) == CharType.IsQuoteLeft)
                                {
                                    if ((ct & CharType.IsQuoteDoubleSided) == CharType.IsQuoteDoubleSided)
                                        _openQuotaDoubleSided = !_openQuotaDoubleSided;
                                    else
                                        _openQuotas++;
                                }
                                else
                                if ((ct & CharType.IsQuoteRight) == CharType.IsQuoteRight)
                                {
                                    if ((ct & CharType.IsQuoteDoubleSided) == CharType.IsQuoteDoubleSided)
                                        _openQuotaDoubleSided = !_openQuotaDoubleSided;
                                    else
                                        _openQuotas--;
                                }
                            }
                            break;
                    }


                    if ((sct & SentCharType.ExcludeInBracketAndQuote) == SentCharType.ExcludeInBracketAndQuote)
                    {
                        if (IsEndOfSentTDQMEP(sct))
                        {
                            SkipFollowPunctuation();
                            CreateSentAndPutToList();
                            continue;
                        }
                        _ptr++;
                        continue;
                    }

                    if (_splitBySmiles && (sct & SentCharType.SmileBegin) == SentCharType.SmileBegin)
                    {
                        var smile_length = TryGetSmileLength();
                        if (0 < smile_length)
                        {
                            _ptr += smile_length;
                            CreateSentAndPutToList();
                            continue;
                        }
                    }

                    if ((sct & SentCharType.Dot) == SentCharType.Dot)
                    {
                        if (IsPreviousAndNextCharDigit())
                        {
                            _ptr++;
                            continue;
                        }

                        if (!TokenizeDotArea())
                        {
                            _ptr++;
                            continue;
                        }

                        var offset = TryBeforeProperOrNumberBeforeNoProper();
                        if (offset.HasValue)
                        {
                            if (offset.Value < 0)
                            {
                                _ptr -= offset.Value;
                            }
                            else
                            {
                                _ptr += offset.Value;
                                CreateSentAndPutToList();
                            }
                            continue;
                        }

                        offset = TryGetThreeDotsLength();
                        if (offset.HasValue)
                        {
                            if (offset.Value < 0)
                            {
                                _ptr -= offset.Value;
                            }
                            else
                            {
                                _ptr += offset.Value;
                                CreateSentAndPutToList();
                            }
                            continue;
                        }

                        offset = TryGetSingleUpperChar();
                        if (offset.HasValue)
                        {
                            if (offset.Value < 0)
                            {
                                _ptr -= offset.Value;
                            }
                            else
                            {
                                _ptr += offset.Value;
                                CreateSentAndPutToList();
                            }
                            continue;
                        }

                        if (IsInterjection())
                        {
                            _ptr++;
                            CreateSentAndPutToList();
                            continue;
                        }

                        var len = TryGetFileExtensionLength();
                        if (0 < len)
                        {
                            _ptr += len;
                            continue;
                        }

                        len = TryGetYandexCombinationsLength();
                        if (0 < len)
                        {
                            _ptr += len;
                            continue;
                        }

                        offset = TryOtherSituation();
                        if (offset.HasValue)
                        {
                            if (offset.Value < 0)
                            {
                                _ptr -= offset.Value;
                            }
                            else
                            {
                                _ptr += offset.Value;
                                CreateSentAndPutToList();
                            }
                            continue;
                        }

                        len = TryEndOfQuotingLength();
                        if (0 < len)
                        {
                            _ptr += len;
                            continue;
                        }

                        len = TryListLength();
                        if (0 < len)
                        {
                            _ptr += len;
                            continue;
                        }

                        _ptr++;
                        CreateSentAndPutToList();
                        continue;
                    }

                    _ptr++;
                }

                CreateLastSentAndPutToList(text.Length);
            }

            return _sentences;
        }

        private void CreateSentAndPutToList()
        {
            var startIndex = _Sent.StartIndex + _Sent.Length;
            var length = (int)(_ptr - _BASE - startIndex);
            if (length <= 0)
                return;

            _Sent.SetLength(length);

            if (!IsCurrentSentContainsPunctuationOrWhitespace())
            {
                _sentences.Add(_Sent);
            }
            var sent = new Sentence(_Sent.StartIndex + _Sent.Length);

            _Sent = sent;
            _openBrakets = 0;
            _openQuotas = 0;
            _openQuotaDoubleSided = false;
        }

        private void CreateLastSentAndPutToList(int text_length)
        {
            var startIndex = _Sent.StartIndex + _Sent.Length;
            var length = text_length - startIndex;
            if (length <= 0)
                return;

            _Sent.SetLength(length);

            if (!IsCurrentSentContainsPunctuationOrWhitespace())
            {
                _sentences.Add(_Sent);
            }
        }

        private void SetSentAndCallback()
        {
            var startIndex = _Sent.StartIndex + _Sent.Length;
            var length = (int)(_ptr - _BASE - startIndex);
            if (length <= 0)
                return;

            _Sent.SetLength(length);

            if (!IsCurrentSentContainsPunctuationOrWhitespace())
            {
                _outerProcessSentCallback_Delegate(_Sent);
            }
            _Sent.Reset(_Sent.StartIndex + _Sent.Length);

            _openBrakets = 0;
            _openQuotas = 0;
            _openQuotaDoubleSided = false;
        }
        private void SetLastSentAndCallback(int text_length)
        {
            var startIndex = _Sent.StartIndex + _Sent.Length;
            var length = text_length - startIndex;
            if (length <= 0)
                return;

            _Sent.SetLength(length);

            if (!IsCurrentSentContainsPunctuationOrWhitespace())
            {
                _outerProcessSentCallback_Delegate(_Sent);
            }
        }

        private bool IsEndOfSentTDQMEP(SentCharType sct)
        {
            if ((sct & SentCharType.ThreeDot) == SentCharType.ThreeDot)
            {
                var wasSomePunctuation = false;
                CharType ct;
                for (var ptr = _ptr + 1; ; ptr++)
                {
                    var ch = *ptr;
                    if (ch == '\0')
                        return true;

                    ct = *(_CTM + ch);
                    if ((ct & CharType.IsBracketRight) == CharType.IsBracketRight)
                    {
                        if (wasSomePunctuation)
                            return true;
                        return _openBrakets <= 0;
                    }
                    if ((*(_SCTM + ch) & SentCharType.AfterThreeDotAllowedPunctuation) == SentCharType.AfterThreeDotAllowedPunctuation)
                    {
                        wasSomePunctuation = true;
                    }
                    else
                    if ((ct & CharType.IsWhiteSpace) != CharType.IsWhiteSpace)
                    {
                        break;
                    }
                }

                if ((ct & CharType.IsLower) == CharType.IsLower)
                {
                    return false;
                }
            }
            else
            {
                if (_openBrakets == 1)
                    return false;
                if (_openQuotas == 1)
                    return false;
                if (_openQuotaDoubleSided)
                    return false;

                for (var ptr = _ptr + 1; ; ptr++)
                {
                    var ch = *ptr;
                    if (ch == '\0')
                        break;
                    var ct = *(_CTM + ch);
                    if ((ct & CharType.IsBracketLeft) == CharType.IsBracketLeft || (ct & CharType.IsBracketRight) == CharType.IsBracketRight)
                    {
                        for (ptr++; ; ptr++)
                        {
                            ch = *ptr;
                            if (ch == '\0')
                            {
                                return false;
                            }
                            if ((*(_SCTM + ch) & SentCharType.AfterBracketAllowedPunctuation4QMEP) == SentCharType.AfterBracketAllowedPunctuation4QMEP)
                            {
                                return false;
                            }
                            ct = *(_CTM + ch);
                            if ((ct & CharType.IsWhiteSpace) != CharType.IsWhiteSpace && (ct & CharType.IsPunctuation) != CharType.IsPunctuation)
                            {
                                return true;
                            }
                        }

                    }
                    if ((ct & CharType.IsWhiteSpace) != CharType.IsWhiteSpace && (ct & CharType.IsPunctuation) != CharType.IsPunctuation)
                    {
                        break;
                    }
                }
            }

            return true;
        }

        private int TryGetSmileLength()
        {
            if ((0 < _openBrakets) && (*(_CTM + *_ptr) & CharType.IsBracketRight) == CharType.IsBracketRight)
            {
                return 0;
            }

            var buffer_index = 0;
            for (var right_offset = 0; ; right_offset++)
            {
                var ch = *(_ptr + right_offset);
                if (ch == '\0')
                {
                    break;
                }

                if ((*(_CTM + ch) & CharType.IsWhiteSpace) == CharType.IsWhiteSpace)
                {
                    continue;
                }

                if (_model.Smiles.ValuesMaxLength <= buffer_index)
                {
                    break;
                }

                *(_bufferPtrBase + buffer_index) = ch;
                buffer_index++;
            }

            if (buffer_index < _model.Smiles.ValuesMinLength)
            {
                return 0;
            }

            switch (_model.Smiles.DiffBetweenMixAndMaxLength)
            {
                case 0:
                    return TryCheckSmileInBuffer(buffer_index);
                case 1:
                    {
                        var _right_offset = TryCheckSmileInBuffer(buffer_index);
                        if (_right_offset != 0)
                            return (_right_offset);
                        return TryCheckSmileInBuffer(buffer_index - 1);
                    }
                default:
                    for (int i = _model.Smiles.ValuesMinLength; i <= buffer_index; i++)
                    {
                        var _right_offset = TryCheckSmileInBuffer(i);
                        if (_right_offset != 0)
                            return _right_offset;
                    }
                    break;
            }

            return 0;
        }

        private int TryCheckSmileInBuffer(int buffer_index)
        {
            if (_model.Smiles.Values.TryGetValue(_stringBuilder.Clear().Append(_buffer, 0, buffer_index).ToString(), out Smile smile))
            {
                if (smile.SpaceBefore && !IsPreviousCharWhitespace())
                {
                    return 0;
                }

                //skip all punctuation & whitespace's after/with smile
                for (buffer_index++; ; buffer_index++)
                {
                    var ch = *(_ptr + buffer_index);
                    if (ch == '\0')
                    {
                        break;
                    }

                    var ct = *(_CTM + ch);
                    if ((ct & CharType.IsWhiteSpace) != CharType.IsWhiteSpace && (ct & CharType.IsPunctuation) != CharType.IsPunctuation)
                    {
                        break;
                    }
                }

                buffer_index--;
                return (buffer_index);
            }

            return 0;
        }
        private bool IsPreviousCharWhitespace()
        {
            if (_ptr == _BASE)
                return true;

            var ct = *(_CTM + *(_ptr - 1));
            if ((ct & CharType.IsWhiteSpace) == CharType.IsWhiteSpace)
                return true;

            return false;
        }

        private void SkipFollowPunctuation()
        {
            for (_ptr++; ; _ptr++)
            {
                var ch = *_ptr;
                if ((ch == '\0') ||
                    (*(_CTM + ch) & CharType.IsPunctuation) != CharType.IsPunctuation ||
                    (*(_SCTM + ch) & SentCharType.Unconditional) == SentCharType.Unconditional)
                {
                    return;
                }
            }
        }

        private bool IsPreviousAndNextCharDigit()
        {
            if ((*(_CTM + *(_ptr + 1)) & CharType.IsDigit) == CharType.IsDigit)
            {
                if (_BASE == _ptr)
                {
                    return true;
                }
                if ((*(_CTM + *(_ptr - 1)) & CharType.IsDigit) == CharType.IsDigit)
                {
                    return true;
                }
            }
            return false;
        }

        private bool TokenizeDotArea()
        {
            _dotArea.Reset();

            #region to left
            var leftBorder = GetMaxPtr(_BASE + _Sent.StartIndex, _endUrlPtr);
            if (leftBorder < _ptr)
            {
                var checkUnstickFromDigits = false;
                for (char* start = _ptr, left_ptr = start - 1; ; left_ptr--)
                {
                    #region start of sentence
                    if (left_ptr <= leftBorder)
                    {
                        #region create word
                        var ct0 = *(_CTM + *left_ptr);
                        if ((ct0 & CharType.IsWhiteSpace) == CharType.IsWhiteSpace)
                        {
                            left_ptr++;
                        }

                        var length = (int)(start - left_ptr);
                        if (0 < length)
                        {
                            if ((ct0 & CharType.IsPunctuation) == CharType.IsPunctuation)
                            {
                                var word = new SsWord(left_ptr + 1, length);
                                _dotArea.InsertToHead(word);

                                //punctuation
                                word = new SsWord(left_ptr, 1);
                                _dotArea.InsertToHead(word);
                            }
                            else
                            {
                                var word = new SsWord(left_ptr, length + 1);
                                _dotArea.InsertToHead(word);
                            }
                        }
                        #endregion

                        break;
                    }
                    #endregion

                    var ch = *left_ptr;
                    var ct = *(_CTM + ch);
                    if (ch == '.')
                    {
                        checkUnstickFromDigits = false;
                        #region create word
                        var length = (int)(start - left_ptr);
                        if (0 < length)
                        {
                            var word = new SsWord(left_ptr + 1, length);
                            _dotArea.InsertToHead(word);
                            if (_NgramMaxLengthToLeft <= _dotArea._wordsCount)
                                break;
                            start = left_ptr;
                        }
                        #endregion
                    }
                    else
                    if ((ct & CharType.IsWhiteSpace) == CharType.IsWhiteSpace)
                    {
                        checkUnstickFromDigits = false;
                        #region create word
                        var length = (int)(start - left_ptr);
                        if (0 < length)
                        {
                            var word = new SsWord(left_ptr + 1, length);
                            _dotArea.InsertToHead(word);
                            if (_NgramMaxLengthToLeft <= _dotArea._wordsCount)
                                break;
                            start = left_ptr - 1;
                        }
                        else
                        {
                            start = left_ptr;
                        }
                        #endregion
                    }
                    else
                    if (Xlat.IsDegree(ch))
                    {
                        checkUnstickFromDigits = false;
                        #region create word
                        var length = (int)(start - left_ptr);
                        if (0 < length)
                        {
                            var word = new SsWord(left_ptr + 1, length);
                            _dotArea.InsertToHead(word);
                            if (_NgramMaxLengthToLeft <= _dotArea._wordsCount)
                                break;
                            start = left_ptr - 1;
                        }
                        else
                        {
                            start = left_ptr;
                        }

                        var degreeWord = new SsWord(left_ptr, 1);
                        _dotArea.InsertToHead(degreeWord);
                        if (_NgramMaxLengthToLeft <= _dotArea._wordsCount)
                            break;
                        #endregion
                    }
                    else
                    if ((ct & CharType.IsPunctuation) == CharType.IsPunctuation && (ct & CharType.IsHyphen) != CharType.IsHyphen)
                    {
                        checkUnstickFromDigits = false;
                        #region create word
                        var length = (int)(start - left_ptr);
                        if (0 < length)
                        {
                            var word = new SsWord(left_ptr + 1, length);
                            _dotArea.InsertToHead(word);
                            if (_NgramMaxLengthToLeft <= _dotArea._wordsCount)
                                break;
                            start = left_ptr;
                        }
                        #endregion
                    }
                    else
                    if ((ct & CharType.IsDigit) == CharType.IsDigit)
                    {
                        if (!checkUnstickFromDigits)
                        {
                            var length = (int)(start - left_ptr);
                            if ((1 < length) && Xlat.IsDot(*start))
                            {
                                checkUnstickFromDigits = true;

                                var p = left_ptr + 1;
                                _stringBuilder.Clear();
                                switch (length)
                                {
                                    case 2: _stringBuilder.Append(*p).Append(*(++p)); break;
                                    case 3: _stringBuilder.Append(*p).Append(*(++p)).Append(*(++p)); break;
                                    case 4: _stringBuilder.Append(*p).Append(*(++p)).Append(*(++p)).Append(*(++p)); break;
                                    case 5: _stringBuilder.Append(*p).Append(*(++p)).Append(*(++p)).Append(*(++p)).Append(*(++p)); break;
                                    case 6: _stringBuilder.Append(*p).Append(*(++p)).Append(*(++p)).Append(*(++p)).Append(*(++p)).Append(*(++p)); break;
                                    default:
                                        for (; p <= start; p++)
                                        {
                                            _stringBuilder.Append(*p);
                                        }
                                        break;
                                }
                                if (_model.UnstickFromDigits.Contains(_stringBuilder.ToString()))
                                {
                                    #region create word
                                    var word = new SsWord(left_ptr + 1, length);
                                    _dotArea.InsertToHead(word);
                                    if (_NgramMaxLengthToLeft <= _dotArea._wordsCount)
                                        break;
                                    start = left_ptr;
                                    #endregion
                                }
                            }
                        }
                    }
                    else
                    {
                        checkUnstickFromDigits = false;
                    }
                }
            }
            #endregion

            #region to right
            _dotArea.FixupLeftWord();
            var wordsToRight = 0;
            for (char* start = _ptr + 1, right_ptr = start; ; right_ptr++)
            {
                var ch = *right_ptr;
                #region end of text
                if (ch == '\0')
                {
                    #region create word
                    var length = (int)(right_ptr - start);
                    if (0 < length)
                    {
                        var word = new SsWord(start, length);
                        _dotArea.Add(word);
                    }
                    #endregion

                    break;
                }
                #endregion

                var ct = *(_CTM + ch);
                if (ch == '.')
                {
                    #region create word
                    var length = (int)(right_ptr - start);
                    if (0 < length)
                    {
                        if (*start == '.')
                        {
                            for (right_ptr++; ; right_ptr++)
                            {
                                ch = *right_ptr;
                                ct = *(_CTM + ch);
                                if (((ct & CharType.IsPunctuation) != CharType.IsPunctuation
                                    && (ct & CharType.IsWhiteSpace) != CharType.IsWhiteSpace) ||
                                    ((*(_SCTM + ch) & SentCharType.Unconditional) == SentCharType.Unconditional))
                                {
                                    break;
                                }
                            }
                            right_ptr--;
                            length = (int)(right_ptr - start);
                        }

                        var word = new SsWord(start, length + 1);

                        _dotArea.Add(word);
                        if (_NgramMaxLengthToRight <= ++wordsToRight)
                            break;
                        start = right_ptr + 1;
                    }
                    else
                    {
                        start = right_ptr;
                    }
                    #endregion
                }
                else
                if ((ct & CharType.IsWhiteSpace) == CharType.IsWhiteSpace)
                {
                    #region create word
                    var length = (int)(right_ptr - start);
                    if (0 < length)
                    {
                        var word = new SsWord(start, length);

                        _dotArea.Add(word);
                        if (_NgramMaxLengthToRight <= ++wordsToRight)
                            break;
                    }
                    start = right_ptr + 1;
                    #endregion
                }
                else
                if ((ct & CharType.IsPunctuation) == CharType.IsPunctuation && (ct & CharType.IsHyphen) != CharType.IsHyphen)
                {
                    #region create word
                    var length = (int)(right_ptr - start);
                    if (0 < length)
                    {
                        var word = new SsWord(start, length);

                        _dotArea.Add(word);
                        if (_NgramMaxLengthToRight <= ++wordsToRight)
                            break;
                        start = right_ptr;
                    }
                    #endregion
                }
            }
            _dotArea.FixupRightWord();
            #endregion

            return _dotArea.HasWords;
        }

        private int? TryGetThreeDotsLength()
        {
            /*
            многоточие
            3)	многоточие + пунктуация + маленькая буква = не конец
            */

            if (_dotArea.HasRightWord)
            {
                var wordValue = _dotArea._rightWord.valueOriginal;
                var sct = *(_SCTM + wordValue[0]);
                if (wordValue.Length == 1)
                {
                    if ((sct & SentCharType.Dot) == SentCharType.Dot)
                    {
                        if (_dotArea._rightWord.HasNext)
                        {
                            var next = _dotArea._rightWord.next;
                            wordValue = next.valueOriginal;
                            if ((*(_SCTM + wordValue[0]) & SentCharType.Dot) == SentCharType.Dot)
                            {
                                var offset = (int)(next.EndPtr() - _ptr);

                                if (next.HasNext)
                                {
                                    wordValue = next.next.valueOriginal;
                                    if ((*(_CTM + wordValue[0]) & CharType.IsLower) == CharType.IsLower)
                                    {
                                        return -offset;
                                    }
                                }

                                return offset;
                            }
                        }
                    }
                }
                else
                if ((sct & SentCharType.Dot) == SentCharType.Dot && (*(_SCTM + wordValue[1]) & SentCharType.Dot) == SentCharType.Dot)
                {
                    var offset = (int)(_dotArea._rightWord.EndPtr() - _ptr);

                    if (_dotArea._rightWord.HasNext)
                    {
                        wordValue = _dotArea._rightWord.next.valueOriginal;
                        if ((*(_CTM + wordValue[0]) & CharType.IsLower) == CharType.IsLower)
                        {
                            return -offset;
                        }
                    }

                    return offset;
                }
            }
            return null;
        }

        private int? TryBeforeProperOrNumberBeforeNoProper()
        {
            var sr_pon = default(SearchResultFromHead2Left<BeforeProperOrNumber>?);
            /*
            слева (от точки) стоит слово из BeforeProperOrNumber – не конец.
            исключение: 
                1)	токен из BeforeProperOrNumber одно из  «г. | п. | с. | м.», а за ними большая буквы, тогда смотреть Слово* перед BeforeProperOrNumber, 
                    и если оно состоит из комбинации больших букв IVXCLM или это цифры - конец
            */
            var pon = _model.BeforeProperOrNumberSearcher.FindFromHead2Left(_dotArea._leftWord);
            if (0 < pon.Count)
            {
                var sr = pon.First();
                if (sr.Value.DigitsBeforeOrSlashBefore && sr.LastWord.HasNext)
                {
                    var nextWordValue = sr.LastWord.next.valueOriginal;
                    if ((*(_CTM + nextWordValue[0]) & CharType.IsUpper) == CharType.IsUpper && IsLowerCharsAfterFirstChar(nextWordValue))
                    {
                        if (_dotArea._leftWord.HasPrev)
                        {
                            var prevWordValue = _dotArea._leftWord.prev.valueOriginal;
                            if ((sr.Value.DigitsBefore && IsDigitsOnlyOrRomanDigitsOnly(prevWordValue)) ||
                                (sr.Value.SlashBefore && Xlat.IsSlash(prevWordValue[prevWordValue.Length - 1])))
                            {
                                var offset = (int)(sr.LastWord.EndPtr() - _ptr);
                                return offset;
                            }
                        }
                    }
                }
                sr_pon = sr;
            }

            //слово из списка BeforeNoProper, за которым следует Большая буква – конец;
            var np = _model.BeforeNoProperSearcher.FindFromHead2Left(_dotArea._leftWord);
            if (0 < np.Count)
            {
                var sr_np = np.First();
                if (sr_pon.HasValue && (sr_np.Length < sr_pon.Value.Length))
                {
                    var offset = (int)(sr_pon.Value.LastWord.EndPtr() - _ptr);
                    return -offset;
                }
                else
                {
                    if (sr_np.LastWord.HasNext)
                    {
                        var nextWordValue = sr_np.LastWord.next.valueOriginal;
                        if ((*(_CTM + nextWordValue[0]) & CharType.IsUpper) == CharType.IsUpper)
                        {
                            var offset = (int)(sr_np.LastWord.EndPtr() - _ptr);
                            return offset;
                        }
                    }

                    //sent-not-end
                    var offset2 = (int)(sr_np.LastWord.EndPtr() - _ptr);
                    return -offset2;
                }
            }
            else if (sr_pon.HasValue)
            {
                var offset = (int)(sr_pon.Value.LastWord.EndPtr() - _ptr);
                return -offset;
            }

            return null;
        }

        private int? TryGetSingleUpperChar()
        {
            /*
            Одна большая буква с точкой – не конец.
                исключения:
                1) справа слово или большая буква др. алфавитного диапазона (кириллица / латиница)          –  конец
                2) перед буквой есть слово более 3 букв с заглавной буквы и за ней слово с заглавной буквы  –  конец
                3) буква из множества I|V|X, за которыми слово с заглавной буквы                            –  конец
                4) ºC.|F.|С.|Ф. - если перед буквами C.F.С.Ф. стоит значек º                                –  конец
             (слово – любое сочетание из 3 букв и более, может быть с заглавной буквы, незаглавной и смешанным написанием заглавных и незаглавных букв)
            */

            if (_dotArea.HasLeftWord)
            {
                if (_dotArea._leftWord.valueOriginal.Length == 2)
                {
                    var wordFirstChar = _dotArea._leftWord.valueOriginal[0];
                    if ((*(_CTM + wordFirstChar) & CharType.IsUpper) == CharType.IsUpper)
                    {
                        var rightWordLength = default(int);
                        var rightWordFirstCharIsUpper = default(bool);
                        if (_dotArea.HasRightWord)
                        {
                            var rightWordValue = _dotArea._rightWord.valueOriginal;
                            rightWordLength = rightWordValue.Length;
                            rightWordFirstCharIsUpper = (*(_CTM + rightWordValue[0]) & CharType.IsUpper) == CharType.IsUpper;
                        }

                        #region 1) справа слово или большая буква др. алфавитного диапазона (кириллица / латиница) – конец
                        if (rightWordFirstCharIsUpper)
                        {
                            var left_cst = GetCharsSetType(_dotArea._leftWord.valueOriginal);
                            switch (left_cst)
                            {
                                case CharSetType.CyrillicOnly:
                                    {
                                        var right_cst = GetCharsSetType(_dotArea._rightWord.valueOriginal);
                                        if (right_cst == CharSetType.LatinOnly)
                                        {
                                            return 1;
                                        }
                                    }
                                    break;

                                case CharSetType.LatinOnly:
                                    {
                                        var right_cst = GetCharsSetType(_dotArea._rightWord.valueOriginal);
                                        if (right_cst == CharSetType.CyrillicOnly)
                                        {
                                            return 1;
                                        }
                                    }
                                    break;
                            }
                        }
                        #endregion

                        var prevWordValue = default(string);
                        var prevWordLength = default(int);
                        var prevWordFirstCharIsUpper = default(bool);
                        if (_dotArea._leftWord.HasPrev)
                        {
                            prevWordValue = _dotArea._leftWord.prev.valueOriginal;
                            prevWordLength = prevWordValue.Length;
                            prevWordFirstCharIsUpper = (*(_CTM + prevWordValue[0]) & CharType.IsUpper) == CharType.IsUpper;
                        }

                        #region 2) перед буквой есть слово более 3 букв с заглавной буквы, !но это не первое слово в предложении!, и за ней слово с заглавной буквы – конец
                        if ((3 < prevWordLength) && prevWordFirstCharIsUpper &&
                            (3 <= rightWordLength) && rightWordFirstCharIsUpper &&
                            !IsFirstWordInSent(_dotArea._leftWord.prev))
                        {
                            return 1;
                        }
                        #endregion

                        #region 3) буква из множества I|V|X, за которыми слово с заглавной буквы – конец
                        if (rightWordFirstCharIsUpper)
                        {
                            switch (wordFirstChar)
                            {
                                case 'I':
                                case 'V':
                                case 'X':
                                    if (3 <= rightWordLength)
                                    {
                                        return (1);
                                    }
                                    break;
                            }
                        }
                        #endregion

                        #region 4) º C.|F.|С.|Ф. - если перед буквами C.F.С.Ф. стоит значек º  – конец
                        if (prevWordValue != null)
                        {
                            switch (wordFirstChar)
                            {
                                case 'C':
                                case 'F':
                                case 'С':
                                case 'Ф':
                                    if (Xlat.IsDegree(prevWordValue[0]))
                                    {
                                        return 1;
                                    }
                                    break;
                            }
                        }
                        #endregion

                        return -1;
                    }
                }
            }

            return null;
        }

        private int? TryOtherSituation()
        {
            /*
            слева сочетание букв (любых), заканчивающихся на цифру, справа:
                1) большая   буква (или [_СЛОВО_] с большой   буквы) – конец.
                2) маленькая буква (или [_СЛОВО_] с маленькой буквы) – не конец.
             
            слева все большие буквы (больше одной), справа [_СЛОВО_] с:
                1) большой буквы    –  конец.
                2) маленькой буквы  –  не конец.            
             
            слева [_СЛОВО_] с большой  буквы не более 2 (уменьшим не единицу!!!) букв, справа [_СЛОВО_] с большой  буквы – не конец.
                исключения:
                1) справа и слева [_СЛОВО_] разного алфавитного диапазона (кириллица / латиница) - конец.
             
            ([_СЛОВО_] – любое сочетание 3 букв и более, может быть с заглавной буквы, незаглавной и смешанным написанием заглавных и незаглавных букв.)
            */

            if (_dotArea.HasLeftWord && _dotArea.HasRightWord)
            {
                if (1 < _dotArea._leftWord.valueOriginal.Length)
                {
                    //слева сочетание букв (любых), заканчивающихся на цифру
                    if (IsLettersEndsWithDigit(_dotArea._leftWord.valueOriginal))
                    {
                        //справа:
                        var rightWordStartUpper = (*(_CTM + _dotArea._rightWord.valueOriginal[0]) & CharType.IsUpper) == CharType.IsUpper;
                        //1) большая   буква (или [_СЛОВО_] с большой   буквы) – конец.
                        //2) маленькая буква (или [_СЛОВО_] с маленькой буквы) – не конец.
                        return rightWordStartUpper ? 1 : -1;
                    }

                    var wordValue = _dotArea._leftWord.valueOriginal;
                    if ((*(_CTM + wordValue[0]) & CharType.IsUpper) == CharType.IsUpper)
                    {
                        //справа [_СЛОВО_]
                        if (3 <= _dotArea._rightWord.valueOriginal.Length)
                        {
                            var rightWordStartUpper = (*(_CTM + _dotArea._rightWord.valueOriginal[0]) & CharType.IsUpper) == CharType.IsUpper;

                            //слева все большие буквы (больше одной)
                            if (IsUpperCharsAfterFirstChar(wordValue))
                            {
                                //большой буквы  –  конец.
                                if (rightWordStartUpper)
                                {
                                    return 1;
                                }
                                //маленькой буквы  –  не конец.
                                //sent-not-end
                                return -1;
                            }
                            //слева слово с большой буквы не более 2 букв
                            else if (rightWordStartUpper && (LengthWithoutStartEndPunctuation(_dotArea._leftWord.valueOriginal) <= 2))
                            {
                                var left_cst = GetCharsSetType(_dotArea._leftWord.valueOriginal);
                                switch (left_cst)
                                {
                                    case CharSetType.CyrillicOnly:
                                        {
                                            var right_cst = GetCharsSetType(_dotArea._rightWord.valueOriginal);
                                            if (right_cst == CharSetType.LatinOnly)
                                            {
                                                return 1;
                                            }
                                        }
                                        break;

                                    case CharSetType.LatinOnly:
                                        {
                                            var right_cst = GetCharsSetType(_dotArea._rightWord.valueOriginal);
                                            if (right_cst == CharSetType.CyrillicOnly)
                                            {
                                                return 1;
                                            }
                                        }
                                        break;
                                }

                                //с большой буквы – не конец
                                return -1;
                            }
                        }
                    }
                }
            }
            return null;
        }
        private bool IsInterjection()
        {
            //междометия с точкой (регистр не важен) – конец
            if (_dotArea.HasLeftWord)
            {
                _stringBuilder.Clear();
                fixed (char* _base = _dotArea._leftWord.valueOriginal)
                {
                    for (var ptr = _base; ; ptr++)
                    {
                        var ch = *ptr;
                        if (ch == '\0')
                            break;
                        if ((*(_CTM + ch) & CharType.IsLetter) != CharType.IsLetter)
                            break;
                        _stringBuilder.Append(*(_UIM + ch));
                        if (_model.Interjections.ValuesMaxLength <= _stringBuilder.Length)
                            break;
                    }
                }
                if (_model.Interjections.Values.Contains(_stringBuilder.ToString()))
                {
                    return true;
                }
            }
            return false;
        }

        private int TryGetFileExtensionLength()
        {
            const long ONE_LONG = 1L;

            //расширения файлов (по правилам работы с доменами первого уровня) – не конец
            if (_dotArea.HasRightWord)
            {
                //convert right-word value to-upper
                if (((_dotArea._rightWord.startPtr - _ptr) == ONE_LONG) &&
                    (_dotArea._rightWord.valueOriginal.Length <= _model.FileExtensions.ValuesMaxLength))
                {
                    if (_dotArea._rightWord.valueUpper == null)
                    {
                        _stringBuilder.Clear();
                        fixed (char* _base = _dotArea._rightWord.valueOriginal)
                        {
                            for (var ptr = _base; *ptr != '\0'; ptr++)
                            {
                                _stringBuilder.Append(*(_UIM + *ptr));
                            }
                        }
                        _dotArea._rightWord.valueUpper = _stringBuilder.ToString();
                    }
                    if (_model.FileExtensions.Values.Contains(_dotArea._rightWord.valueUpper))
                    {
                        return _dotArea._rightWord.valueUpper.Length + 1;
                    }
                }
            }
            return 0;
        }

        private int TryGetYandexCombinationsLength()
        {
            //Яндекс.сочетания (точка вплотную к яндексу, регистр не важен) – не конец
            if (_dotArea.HasLeftWord && _dotArea.HasRightWord)
            {
                //convert left-word value to-upper
                if (_dotArea._leftWord.valueOriginal.Length != SentSplitterModel.YANDEX_EN.Length &&
                    _dotArea._leftWord.valueOriginal.Length != SentSplitterModel.YANDEX_RU.Length)
                {
                    return 0;
                }
                if (_dotArea._leftWord.valueUpper == null)
                {
                    _stringBuilder.Clear();
                    fixed (char* _base = _dotArea._leftWord.valueOriginal)
                    {
                        for (var ptr = _base; *ptr != '\0'; ptr++)
                        {
                            _stringBuilder.Append(*(_UIM + *ptr));
                        }
                    }
                    _dotArea._leftWord.valueUpper = _stringBuilder.ToString();
                }
                switch (_dotArea._leftWord.valueUpper)
                {
                    case SentSplitterModel.YANDEX_RU:
                    case SentSplitterModel.YANDEX_EN: break;
                    default:
                        return 0;
                }

                _stringBuilder.Clear();
                fixed (char* _base = _dotArea._rightWord.valueOriginal)
                {
                    for (var ptr = _base; ; ptr++)
                    {
                        var ch = *ptr;
                        if (ch == '\0')
                            break;
                        if ((*(_CTM + ch) & CharType.IsLetter) != CharType.IsLetter)
                            break;
                        _stringBuilder.Append(*(_UIM + ch));
                        if (_model.YandexCombinations.ValuesMaxLength <= _stringBuilder.Length)
                            break;
                    }
                }
                if (_model.YandexCombinations.Values.Contains(_stringBuilder.ToString()))
                {
                    var offset = (int)(_dotArea._rightWord.startPtr + _stringBuilder.Length - _ptr);
                    return offset;
                }
            }
            return 0;
        }

        private int TryEndOfQuotingLength()
        {
            //сочетание символов .“ – (справа символы пунктуации “ –) или любой из символов ,;:   –  не конец
            if (_dotArea.HasRightWord)
            {
                var wordValue = _dotArea._rightWord.valueOriginal;

                var firstChar = wordValue[0];
                switch (firstChar)
                {
                    case ',':
                    case ';':
                    case ':':
                        return 1;
                }

                if (wordValue.Length == 1)
                {
                    if ((*(_CTM + firstChar) & CharType.IsQuote) == CharType.IsQuote)
                    {
                        if (_dotArea._rightWord.HasNext)
                        {
                            if ((*(_CTM + _dotArea._rightWord.next.valueOriginal[0]) & CharType.IsHyphen) == CharType.IsHyphen) //if ( xlat.IsHyphen( _DotVicinity._RightWord.next.valueOriginal[ 0 ] ) )
                            {
                                var offset = (int)(_dotArea._rightWord.next.EndPtr() - _ptr);
                                return offset;
                            }
                        }
                    }
                }
                else
                if ((*(_CTM + firstChar) & CharType.IsQuote) == CharType.IsQuote &&
                    (*(_CTM + wordValue[1]) & CharType.IsHyphen) == CharType.IsHyphen)
                {
                    var offset = (int)(_dotArea._rightWord.EndPtr() - _ptr);
                    return offset;
                }
            }
            return 0;
        }

        private int TryListLength()
        {
            /*
            списки:
            9.	предложение, состоящее только из цифр с точкой на конце присоединять к следующему предложению.
            */

            if (_dotArea.HasLeftWord && !_dotArea._leftWord.HasPrev)
            {
                if (IsDigitsOnlyWithoutLastDot(_dotArea._leftWord.valueOriginal))
                {
                    return 1;
                }
            }
            return 0;
        }

        private bool IsDigitsOnlyOrRomanDigitsOnly(string value)
        {
            fixed (char* _base = value)
            {
                return IsDigitsOnly(_base) || IsRomanDigitsOnly(_base);
            }
        }
        private bool IsDigitsOnlyWithoutLastDot(string value)
        {
            fixed (char* _base = value)
            {
                var ptr = _base;
                for (int i = 0, len = value.Length - 1; i < len; i++)
                {
                    if ((*(_CTM + *(ptr + i)) & CharType.IsDigit) != CharType.IsDigit)
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        private bool IsUpperCharsAfterFirstChar(string value)
        {
            fixed (char* _base = value)
            {
                return IsUpperChars(_base + 1);
            }
        }

        private bool IsLowerCharsAfterFirstChar(string value)
        {
            fixed (char* _base = value)
            {
                return IsLowerChars(_base + 1);
            }
        }

        unsafe private bool IsDigitsOnly(char* ptr)
        {
            for (; *ptr != '\0'; ptr++)
            {
                if ((*(_CTM + *ptr) & CharType.IsDigit) != CharType.IsDigit)
                {
                    return false;
                }
            }
            return true;
        }

        unsafe private bool IsRomanDigitsOnly(char* ptr)
        {
            for (; *ptr != '\0'; ptr++)
            {
                if ((*(_SCTM + *ptr) & SentCharType.RomanDigit) != SentCharType.RomanDigit)
                {
                    return false;
                }
            }
            return true;
        }

        unsafe private bool IsUpperChars(char* ptr)
        {
            for (; ; ptr++)
            {
                var ch = *ptr;
                if (ch == '\0')
                {
                    break;
                }
                var ct = *(_CTM + ch);
                if ((ct & CharType.IsPunctuation) == CharType.IsPunctuation)
                {
                    continue;
                }
                if ((ct & CharType.IsUpper) != CharType.IsUpper)
                {
                    return false;
                }
            }
            return true;
        }

        unsafe private bool IsLowerChars(char* ptr)
        {
            for (; ; ptr++)
            {
                var ch = *ptr;
                if (ch == '\0')
                {
                    break;
                }
                var ct = *(_CTM + ch);
                if ((ct & CharType.IsPunctuation) == CharType.IsPunctuation)
                {
                    continue;
                }
                if ((ct & CharType.IsLower) != CharType.IsLower)
                {
                    return false;
                }
            }
            return true;
        }

        private int LengthWithoutStartEndPunctuation(string value)
        {
            fixed (char* _base = value)
            {
                var start = _base;
                for (; ; start++)
                {
                    var ch = *start;
                    if (ch == '\0')
                    {
                        break;
                    }
                    if ((*(_CTM + ch) & CharType.IsPunctuation) != CharType.IsPunctuation)
                    {
                        break;
                    }
                }

                var end = _base + value.Length - 1;
                for (; ; end--)
                {
                    if (end <= start)
                    {
                        break;
                    }
                    if ((*(_CTM + *end) & CharType.IsPunctuation) != CharType.IsPunctuation)
                    {
                        break;
                    }
                }

                return (int)(end + 1 - start);
            }
        }

        [Flags]
        private enum CharSetType
        {
            __UNDEFINE__ = 0,

            CyrillicOnly = 0x1,
            LatinOnly = 1 << 1,
        }

        private CharSetType GetCharsSetType(string value)
        {
            var cst = default(CharSetType);
            fixed (char* _base = value)
            {
                for (var ptr = _base; ; ptr++)
                {
                    var ch = *ptr;
                    if (ch == '\0')
                        break;
                    if ((*(_CTM + ch) & CharType.IsLetter) != CharType.IsLetter)
                        continue;
                    if (0 <= ch && ch <= 127)
                    {
                        cst |= CharSetType.LatinOnly;
                    }
                    else
                    if ('А' <= ch && ch <= 'я')
                    {
                        cst |= CharSetType.CyrillicOnly;
                    }
                }
            }
            return cst;
        }

        private bool IsLettersEndsWithDigit(string value)
        {
            fixed (char* _base = value)
            {
                var ptr = _base + value.Length - 1;
                for (; _base <= ptr; ptr--)
                {
                    if ((*(_CTM + *ptr) & CharType.IsPunctuation) != CharType.IsPunctuation)
                        break;
                }
                if ((*(_CTM + *ptr) & CharType.IsDigit) == CharType.IsDigit && (*(_CTM + *_base) & CharType.IsLetter) == CharType.IsLetter)
                {
                    return true;
                }
                return false;
            }
        }

        private bool IsCurrentSentContainsPunctuationOrWhitespace()
        {
            switch (_Sent.Length)
            {
                case 1:
                    {
                        var ct = *(_CTM + *(_BASE + _Sent.StartIndex));
                        if ((ct & CharType.IsWhiteSpace) != CharType.IsWhiteSpace && (ct & CharType.IsPunctuation) != CharType.IsPunctuation)
                        {
                            return false;
                        }
                        return true;
                    }

                case 2:
                    {
                        var p = _BASE + _Sent.StartIndex;
                        var ct = *(_CTM + *p);
                        if ((ct & CharType.IsWhiteSpace) != CharType.IsWhiteSpace && (ct & CharType.IsPunctuation) != CharType.IsPunctuation)
                        {
                            return false;
                        }
                        ct = *(_CTM + *(++p));
                        if ((ct & CharType.IsWhiteSpace) != CharType.IsWhiteSpace && (ct & CharType.IsPunctuation) != CharType.IsPunctuation)
                        {
                            return false;
                        }
                        return true;
                    }

                case 3:
                    {
                        var p = _BASE + _Sent.StartIndex;
                        var ct = *(_CTM + *p);
                        if ((ct & CharType.IsWhiteSpace) != CharType.IsWhiteSpace && (ct & CharType.IsPunctuation) != CharType.IsPunctuation)
                        {
                            return false;
                        }
                        ct = *(_CTM + *(++p));
                        if ((ct & CharType.IsWhiteSpace) != CharType.IsWhiteSpace && (ct & CharType.IsPunctuation) != CharType.IsPunctuation)
                        {
                            return false;
                        }
                        ct = *(_CTM + *(++p));
                        if ((ct & CharType.IsWhiteSpace) != CharType.IsWhiteSpace && (ct & CharType.IsPunctuation) != CharType.IsPunctuation)
                        {
                            return false;
                        }
                        return true;
                    }

                case 4:
                    {
                        var p = _BASE + _Sent.StartIndex;
                        var ct = *(_CTM + *p);
                        if ((ct & CharType.IsWhiteSpace) != CharType.IsWhiteSpace && (ct & CharType.IsPunctuation) != CharType.IsPunctuation)
                        {
                            return false;
                        }
                        ct = *(_CTM + *(++p));
                        if ((ct & CharType.IsWhiteSpace) != CharType.IsWhiteSpace && (ct & CharType.IsPunctuation) != CharType.IsPunctuation)
                        {
                            return false;
                        }
                        ct = *(_CTM + *(++p));
                        if ((ct & CharType.IsWhiteSpace) != CharType.IsWhiteSpace && (ct & CharType.IsPunctuation) != CharType.IsPunctuation)
                        {
                            return false;
                        }
                        ct = *(_CTM + *(++p));
                        if ((ct & CharType.IsWhiteSpace) != CharType.IsWhiteSpace && (ct & CharType.IsPunctuation) != CharType.IsPunctuation)
                        {
                            return false;
                        }
                        return true;
                    }

                default:
                    for (int i = _Sent.StartIndex, end = i + _Sent.Length; i < end; i++)
                    {
                        var ct = *(_CTM + *(_BASE + i));
                        if ((ct & CharType.IsWhiteSpace) != CharType.IsWhiteSpace && (ct & CharType.IsPunctuation) != CharType.IsPunctuation)
                        {
                            return false;
                        }
                    }
                    return true;
            }
        }

        private bool IsFirstWordInSent(SsWord word)
        {
            if (word.HasPrev)
            {
                for (char* ptr = word.startPtr - 1, leftBorder = GetMaxPtr(_BASE + _Sent.StartIndex, _endUrlPtr); leftBorder <= ptr; ptr--)
                {
                    var ct = *(_CTM + *ptr);
                    if ((ct & CharType.IsLetter) == CharType.IsLetter || (ct & CharType.IsDigit) == CharType.IsDigit)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        unsafe private static char* GetMaxPtr(char* p1, char* p2)
        {
            return p1 > p2 ? p1 : p2;
        }
    }
}
