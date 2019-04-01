using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using LangAnalyzer.Core;
using LangAnalyzer.Ner;
using LangAnalyzer.Postagger;
using LangAnalyzer.SentenceSplitter;
using LangAnalyzer.Urls;

namespace LangAnalyzer.Tokenizing
{
    unsafe sealed public class Tokenizer : IDisposable
    {
        [Flags]
        private enum CRFCharType : byte
        {
            __UNDEFINE__ = 0x0,
            InterpreteAsWhitespace = 0x1,
            BetweenLetterOrDigit = (1 << 1),
            BetweenDigit = (1 << 2),
            TokenizeDifferentSeparately = (1 << 3),
            DotChar = (1 << 4)
        }

        unsafe private sealed class UnsafeConst
        {
            #region static & xlat table's
            public static readonly char* MAX_PTR = (char*)(0xffffffffFFFFFFFF);
            private const string INCLUDE_INTERPRETE_AS_WHITESPACE = "¤¦§¶";
            private const char DOT = '\u002E';
            private static readonly char[] BETWEEN_LETTER_OR_DIGIT = new char[] {
                '\u0026', /* 0x26  , 38  , '&' */
                '\u0027', /* 0x27  , 39  , ''' */
                '\u002D', /* 0x2D  , 45  , '-' */
                '\u005F', /* 0x5F  , 95  , '_' */
                '\u00AD', /* 0xAD  , 173 , '­' */
                '\u055A', /* 0x55A , 1370, '՚' */
                '\u055B', /* 0x55B , 1371, '՛' */
                '\u055D', /* 0x55D , 1373, '՝' */
                '\u2012', /* 0x2012, 8210, '‒' */
                '\u2013', /* 0x2013, 8211, '–' */
                '\u2014', /* 0x2014, 8212, '—' */
                '\u2015', /* 0x2015, 8213, '―' */
                '\u2018', /* 0x2018, 8216, '‘' */
                '\u2019', /* 0x2019, 8217, '’' */
                '\u201B', /* 0x201B, 8219, '‛' */
            };
            private static readonly char[] BETWEEN_LETTER_OR_DIGIT_EN = new char[] {
                '\u0026', /* 0x26  , 38  , '&' */
                '\u002D', /* 0x2D  , 45  , '-' */
                '\u005F', /* 0x5F  , 95  , '_' */
                '\u00AD', /* 0xAD  , 173 , '­' */
                '\u2012', /* 0x2012, 8210, '‒' */
                '\u2013', /* 0x2013, 8211, '–' */
                '\u2014', /* 0x2014, 8212, '—' */
                '\u2015', /* 0x2015, 8213, '―' */
                '\u2018', /* 0x2018, 8216, '‘' */
                '\u201B', /* 0x201B, 8219, '‛' */
            };
            private static readonly char[] BETWEEN_DIGIT = new char[] {
                '\u0022', /* 0x22   , 34   , '"'  */
                '\u002C', /* 0x2C   , 44   , ','  */
                '\u003A', /* 0x3A   , 58   , ':'  */
                '\u3003' /* 0x3003 , 12291, '〃' */
            };
            private static readonly char[] TOKENIZE_DIFFERENT_SEPARATELY = new char[] {
                '\u2012', /* 0x2012 , 8210 , '‒' */
                '\u2013', /* 0x2013 , 8211 , '–' */
                '\u2014', /* 0x2014 , 8212 , '—' */
                '\u2015', /* 0x2015 , 8213 , '―' */
                '\u2018', /* 0x2018 , 8216 , '‘' */
                '\u2019', /* 0x2019 , 8217 , '’' */
                '\u201B', /* 0x201B , 8219 , '‛' */
                '\u201C', /* 0x201C , 8220 , '“' */
                '\u201D', /* 0x201D , 8221 , '”' */
                '\u201E', /* 0x201E , 8222 , '„' */
                '\u201F', /* 0x201F , 8223 , '‟' */
                '\u2026', /* 0x2026 , 8230 , '…' */
                '\u0021', /* 0x21   , 33   , '!' */
                '\u0022', /* 0x22   , 34   , '"' */
                '\u0026', /* 0x26   , 38   , '&' */
                '\u0027', /* 0x27   , 39   , ''' */
                '\u0028', /* 0x28   , 40   , '(' */
                '\u0029', /* 0x29   , 41   , ')' */
                '\u002C', /* 0x2C   , 44   , ',' */
                '\u002D', /* 0x2D   , 45   , '-' */
                '\u3003', /* 0x3003 , 12291, '〃' */
                '\u003A', /* 0x3A   , 58   , ':' */
                '\u003B', /* 0x3B   , 59   , ';' */
                '\u003F', /* 0x3F   , 63   , '?' */
                '\u055A', /* 0x55A  , 1370 , '՚' */
                '\u055B', /* 0x55B  , 1371 , '՛'  */
                '\u055D', /* 0x55D  , 1373 , '՝' */
                '\u005B', /* 0x5B   , 91   , '[' */
                '\u005D', /* 0x5D   , 93   , ']' */
                '\u005F', /* 0x5F   , 95   , '_' */
                '\u05F4', /* 0x5F4  , 1524 , '״' */
                '\u007B', /* 0x7B   , 123  , '{' */
                '\u007D', /* 0x7D   , 125  , '}' */
                '\u00A1', /* 0xA1   , 161  , '¡' */
                '\u00AB', /* 0xAB   , 171  , '«' */
                '\u00AD', /* 0xAD   , 173  , '­' */
                '\u00BB', /* 0xBB   , 187  , '»' */
                '\u00BF', /* 0xBF   , 191  , '¿' */
                '/',
                '¥', '©', '®', '€', '™', '°', '№', '$', '%',
                '<', '>'
            };
            #endregion

            public readonly CRFCharType* _CRF_CHARTYPE_MAP;

            private UnsafeConst(LanguageTypeEnum languageType)
            {
                var CRF_CHARTYPE_MAP = new byte[char.MaxValue + 1];
                fixed (byte* cctm = CRF_CHARTYPE_MAP)
                {
                    for (var c = char.MinValue; ; c++)
                    {
                        if (char.IsPunctuation(c))
                        {
                            *(cctm + c) = (byte)CRFCharType.InterpreteAsWhitespace;
                        }

                        if (c == char.MaxValue)
                        {
                            break;
                        }
                    }

                    foreach (var c in INCLUDE_INTERPRETE_AS_WHITESPACE)
                    {
                        *(cctm + c) = (byte)CRFCharType.InterpreteAsWhitespace;
                    }

                    foreach (var c in TOKENIZE_DIFFERENT_SEPARATELY)
                    {
                        *(cctm + c) = (byte)CRFCharType.TokenizeDifferentSeparately;
                    }

                    var between_letter_or_digit = (languageType == LanguageTypeEnum.En)
                        ? BETWEEN_LETTER_OR_DIGIT_EN
                        : BETWEEN_LETTER_OR_DIGIT;
                    foreach (var c in between_letter_or_digit)
                    {
                        *(cctm + c) |= (byte)CRFCharType.BetweenLetterOrDigit;
                    }

                    foreach (var c in BETWEEN_DIGIT)
                    {
                        *(cctm + c) |= (byte)CRFCharType.BetweenDigit;
                    }

                    *(cctm + DOT) = (byte)CRFCharType.DotChar;
                }

                var CRF_CHARTYPE_MAP_GCHandle = GCHandle.Alloc(CRF_CHARTYPE_MAP, GCHandleType.Pinned);
                _CRF_CHARTYPE_MAP = (CRFCharType*)CRF_CHARTYPE_MAP_GCHandle.AddrOfPinnedObject().ToPointer();
            }

            private static UnsafeConst _Inst_Ru;
            private static UnsafeConst _Inst_En;

            public static UnsafeConst GetInstanceByLanguage(LanguageTypeEnum languageType)
            {
                switch (languageType)
                {
                    case LanguageTypeEnum.En:
                        if (_Inst_En == null)
                        {
                            lock (typeof(UnsafeConst))
                            {
                                if (_Inst_En == null)
                                {
                                    _Inst_En = new UnsafeConst(languageType);
                                }
                            }
                        }
                        return _Inst_En;

                    default:
                        if (_Inst_Ru == null)
                        {
                            lock (typeof(UnsafeConst))
                            {
                                if (_Inst_Ru == null)
                                {
                                    _Inst_Ru = new UnsafeConst(languageType);
                                }
                            }
                        }
                        return _Inst_Ru;
                }
            }
        }

        #region private fields
        private const int DEFAULT_WORDSLIST_CAPACITY = 100;
        private const int DEFAULT_WORDTOUPPERBUFFER = 100;

        private readonly SentSplitter _sentSplitter;
        private readonly List<Word> _words;
        private readonly IPosTaggerInputTypeProcessor _posTaggerInputTypeProcessor;
        private readonly INerInputTypeProcessor _nerInputTypeProcessor;
        private readonly HashSet<string> _particleThatExclusion;
        private readonly CharType* _CTM;
        private readonly char* _UIM;
        private readonly CRFCharType* _CCTM;
        private char* _BASE;
        private char* _ptr;
        private int _startIndex;
        private int _length;
        private ProcessSentCallbackDelegate _outerProcessSentCallbackDelegate;
        private char* _startPtr;
        private char* _endPtr;
        private int _wordToUpperBufferSize;
        private GCHandle _wordToUpperBufferGCHandle;
        private char* _wordToUpperBufferPtrBase;
        private bool _dontSkipNonLetterAndNonDigitToTheEnd;
        private readonly SentSplitter.ProcessSentCallbackDelegate _sentSplitterProcessSentCallbackDelegate;
        #endregion

        #region ctor dtor dispose
        public Tokenizer(TokenizerConfig config)
        {
            _sentSplitter = new SentSplitter(config.SentSplitterConfig);
            _words = new List<Word>(DEFAULT_WORDSLIST_CAPACITY);
            _particleThatExclusion = config.Model.ParticleThatExclusion;
            _sentSplitterProcessSentCallbackDelegate = new SentSplitter.ProcessSentCallbackDelegate(SentSplitterProcessSentCallback);

            _UIM = XlatUnsafe.Inst._UPPER_INVARIANT_MAP;
            _CTM = XlatUnsafe.Inst._CHARTYPE_MAP;
            _CCTM = UnsafeConst.GetInstanceByLanguage(config.LanguageType)._CRF_CHARTYPE_MAP;

            ReAllocWordToUpperBuffer(DEFAULT_WORDTOUPPERBUFFER);

            if ((config.TokenizeMode & TokenizeMode.PosTagger) == TokenizeMode.PosTagger)
            {
                _posTaggerInputTypeProcessor = config.PosTaggerInputTypeProcessorFactory.CreateInstance();
            }
            else
            {
                _posTaggerInputTypeProcessor = DummyPosTaggerInputTypeProcessor.Instance;
            }

            if ((config.TokenizeMode & TokenizeMode.Ner) == TokenizeMode.Ner)
            {
                _nerInputTypeProcessor = config.NerInputTypeProcessorFactory.CreateInstance();
            }
            else
            {
                _nerInputTypeProcessor = DummyNerInputTypeProcessor.Instance;
            }
        }

        private void ReAllocWordToUpperBuffer(int newBufferSize)
        {
            DisposeNativeResources();

            _wordToUpperBufferSize = newBufferSize;
            var wordToUpperBuffer = new char[_wordToUpperBufferSize];
            _wordToUpperBufferGCHandle = GCHandle.Alloc(wordToUpperBuffer, GCHandleType.Pinned);
            _wordToUpperBufferPtrBase = (char*)_wordToUpperBufferGCHandle.AddrOfPinnedObject().ToPointer();
        }

        ~Tokenizer()
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
            if (_wordToUpperBufferPtrBase != null)
            {
                _wordToUpperBufferGCHandle.Free();
                _wordToUpperBufferPtrBase = null;
            }
        }
        #endregion

        public delegate void ProcessSentCallbackDelegate(List<Word> words);

        public void Run(string text, bool splitBySmiles, ProcessSentCallbackDelegate processSentCallback)
        {
            _outerProcessSentCallbackDelegate = processSentCallback;

            fixed (char* _base = text)
            {
                _BASE = _base;

                _sentSplitter.SplitBySmiles = splitBySmiles;
                _sentSplitter.AllocateSents(text, _sentSplitterProcessSentCallbackDelegate);
            }

            _outerProcessSentCallbackDelegate = null;
        }

        private void SentSplitterProcessSentCallback(Sentence sent)
        {
            _words.Clear();
            _startIndex = sent.StartIndex;
            _length = 0;
            _startPtr = _BASE + _startIndex;
            _endPtr = _startPtr + sent.Length - 1;

            var urls = sent.Urls;
            var urlIndex = 0;
            var startUrlPtr = (urls != null) ? (_BASE + urls[0].startIndex) : UnsafeConst.MAX_PTR;

            #region main
            var realyEndPtr = _endPtr;
            _endPtr = SkipNonLetterAndNonDigitToTheEnd();

            for (_ptr = _startPtr; _ptr <= _endPtr; _ptr++)
            {
                #region process allocated url's
                if (startUrlPtr <= _ptr)
                {
                    TryCreateWordAndPut2List();

                    var lenu = urls[urlIndex].length;

                    _ptr = startUrlPtr + lenu - 1;
                    urlIndex++;
                    startUrlPtr = (urlIndex < urls.Count) ? (_BASE + urls[urlIndex].startIndex) : UnsafeConst.MAX_PTR;

                    _startIndex = (int)(_ptr - _BASE + 1);
                    _length = 0;
                    continue;
                }
                #endregion

                var ch = *_ptr;
                var ct = *(_CTM + ch);

                #region whitespace
                if ((ct & CharType.IsWhiteSpace) == CharType.IsWhiteSpace)
                {
                    TryCreateWordAndPut2List();

                    _startIndex++;
                    continue;
                }
                #endregion

                var pct = *(_CCTM + ch);
                #region dot
                if ((pct & CRFCharType.DotChar) == CRFCharType.DotChar &&
                     IsUpperNextChar()
                   )
                {
                    _length++;
                    TryCreateWordAndPut2List();
                    continue;
                }
                #endregion

                #region between-letter-or-digit
                if ((pct & CRFCharType.BetweenLetterOrDigit) == CRFCharType.BetweenLetterOrDigit)
                {
                    if (IsBetweenLetterOrDigit())
                    {
                        _length++;
                    }
                    else
                    {
                        TryCreateWordAndPut2List();

                        #region merge punctuation (with white-spaces)
                        if (!MergePunctuation(ch))
                            break;
                        #endregion

                        //punctuation word
                        TryCreateWordAndPut2List();
                    }

                    continue;
                }
                else
                if ((pct & CRFCharType.BetweenDigit) == CRFCharType.BetweenDigit)
                {
                    if (IsBetweenDigit())
                    {
                        _length++;
                    }
                    else
                    {
                        TryCreateWordAndPut2List();

                        #region merge punctuation (with white-spaces)
                        if (!MergePunctuation(ch))
                            break;
                        #endregion

                        TryCreateWordAndPut2List();
                    }

                    continue;
                }
                #endregion

                #region tokenize-different-separately
                if ((pct & CRFCharType.TokenizeDifferentSeparately) == CRFCharType.TokenizeDifferentSeparately)
                {
                    TryCreateWordAndPut2List();

                    #region merge punctuation (with white-spaces)
                    if (!MergePunctuation(ch))
                        break;
                    #endregion

                    TryCreateWordAndPut2List();

                    continue;
                }
                #endregion

                #region interprete-as-whitespace
                if ((pct & CRFCharType.InterpreteAsWhitespace) == CRFCharType.InterpreteAsWhitespace)
                {
                    TryCreateWordAndPut2List();

                    _startIndex++;
                    continue;
                }
                #endregion

                _length++;
            }
            #endregion

            TryCreateWordAndPut2List();

            #region tail punctuation
            for (_endPtr = realyEndPtr; _ptr <= _endPtr; _ptr++)
            {
                var ch = *_ptr;
                var ct = *(_CTM + ch);
                #region whitespace
                if ((ct & CharType.IsWhiteSpace) == CharType.IsWhiteSpace)
                {
                    TryCreateWordAndPut2List();

                    _startIndex++;
                    continue;
                }
                #endregion

                var nct = *(_CCTM + ch);
                #region tokenize-different-separately
                if ((nct & CRFCharType.TokenizeDifferentSeparately) == CRFCharType.TokenizeDifferentSeparately)
                {
                    TryCreateWordAndPut2List();

                    #region merge punctuation (with white-spaces)
                    if (!MergePunctuation(ch))
                        break;
                    #endregion

                    TryCreateWordAndPut2List();

                    continue;
                }
                #endregion

                #region interprete-as-whitespace
                if ((nct & CRFCharType.InterpreteAsWhitespace) == CRFCharType.InterpreteAsWhitespace)
                {
                    TryCreateWordAndPut2List();

                    _startIndex++;
                    continue;
                }
                #endregion

                _length++;
            }
            #endregion

            #region last punctuation
            TryCreateWordAndPut2List();
            #endregion

            _outerProcessSentCallbackDelegate(_words);
        }

        private void TryCreateWordAndPut2List()
        {
            const int THAT_LENGTH = 2;
            const int DUSH_LENGTH = 1;
            const int PARTICLE_THAT_LENGTH = DUSH_LENGTH + THAT_LENGTH;
            const string THAT_UPPER = "ТО";
            const string THAT_LOWER = "то";
            const string DUSH = "-";
            const char T_RU = 'Т';
            const char O_RU = 'О';

            if (_length != 0)
            {
                #region to upper invariant & pos-tagger-list & etc
                var startPtr = _BASE + _startIndex;

                if (_wordToUpperBufferSize < _length)
                {
                    ReAllocWordToUpperBuffer(_length);
                }
                for (int i = 0; i < _length; i++)
                {
                    *(_wordToUpperBufferPtrBase + i) = *(_UIM + *(startPtr + i));
                }

                var valueUpper = new string(_wordToUpperBufferPtrBase, 0, _length);

                #region detect particle-that
                var len = _length - PARTICLE_THAT_LENGTH;
                if (0 < len)
                {
                    if ((*(_CTM + *(_wordToUpperBufferPtrBase + len)) & CharType.IsHyphen) == CharType.IsHyphen &&
                         (*(_wordToUpperBufferPtrBase + ++len) == T_RU) &&
                         (*(_wordToUpperBufferPtrBase + ++len) == O_RU)
                       )
                    {
                        if (!_particleThatExclusion.Contains(valueUpper))
                        {
                            #region create word with split particle-that
                            len = _length - PARTICLE_THAT_LENGTH;
                            valueUpper = new string(_wordToUpperBufferPtrBase, 0, len);

                            #region create word without particle-that
                            var _valueOriginal = new string(_BASE, _startIndex, len);
                            var _word = new Word()
                            {
                                startIndex = _startIndex,
                                length = len,
                                valueOriginal = _valueOriginal,
                                valueUpper = valueUpper,
                            };
                            #endregion

                            _word.nerInputType = _nerInputTypeProcessor.GetNerInputType(startPtr, len);

                            #region posTaggerInputType
                            {
                                var _result = _posTaggerInputTypeProcessor.GetResult(startPtr, len, _word);
                                _word.posTaggerInputType = _result.posTaggerInputType;
                                _word.posTaggerExtraWordType = _result.posTaggerExtraWordType;
                                _word.posTaggerFirstCharIsUpper = ((*(_CTM + *startPtr) & CharType.IsUpper) == CharType.IsUpper);
                                _word.posTaggerLastValueUpperInNumeralChain = (_result.posTaggerLastValueUpperInNumeralChainIsValueOriginal)
                                                                              ? _word.valueUpper : _result.posTaggerLastValueUpperInNumeralChain;
                            }
                            #endregion

                            _words.Add(_word);

                            #region create word particle-that
                            var startIndex = _startIndex + len;
                            _word = new Word()
                            {
                                startIndex = startIndex,
                                length = DUSH_LENGTH,
                                valueOriginal = DUSH,
                                valueUpper = DUSH,
                                posTaggerInputType = PosTaggerInputType.Dash,
                                posTaggerExtraWordType = PosTaggerExtraWordType.Punctuation
                            };
                            _words.Add(_word);

                            startIndex++;

                            _word = new Word()
                            {
                                startIndex = startIndex,
                                length = THAT_LENGTH,
                                valueOriginal = THAT_LOWER,
                                valueUpper = THAT_UPPER,
                                posTaggerInputType = PosTaggerInputType.O
                            };
                            _words.Add(_word);
                            #endregion

                            _startIndex += _length;
                            _length = 0;

                            return;
                            #endregion
                        }
                    }
                }
                #endregion
                #endregion

                #region create word
                var valueOriginal = new string(_BASE, _startIndex, _length);
                var word = new Word()
                {
                    startIndex = _startIndex,
                    length = _length,
                    valueOriginal = valueOriginal,
                    valueUpper = valueUpper,
                };
                #endregion

                word.nerInputType = _nerInputTypeProcessor.GetNerInputType(_BASE + _startIndex, _length);

                #region posTaggerInputType
                {
                    var result = _posTaggerInputTypeProcessor.GetResult(startPtr, _length, word);
                    word.posTaggerInputType = result.posTaggerInputType;
                    word.posTaggerExtraWordType = result.posTaggerExtraWordType;
                    word.posTaggerFirstCharIsUpper = ((*(_CTM + *startPtr) & CharType.IsUpper) == CharType.IsUpper);
                    word.posTaggerLastValueUpperInNumeralChain = (result.posTaggerLastValueUpperInNumeralChainIsValueOriginal)
                                                                 ? word.valueUpper : result.posTaggerLastValueUpperInNumeralChain;
                }
                #endregion

                _words.Add(word);

                _startIndex += _length;
                _length = 0;
            }
        }

        private char* SkipNonLetterAndNonDigitToTheEnd()
        {
            if (_dontSkipNonLetterAndNonDigitToTheEnd)
                return _endPtr;

            for (char* ptr = _endPtr; _startPtr <= ptr; ptr--)
            {
                var ct = *(_CTM + *ptr);
                if ((ct & CharType.IsLetter) == CharType.IsLetter || (ct & CharType.IsDigit) == CharType.IsDigit)
                {
                    #region if last letter in sentence is upper, dot is the part of sentence
                    if ((ct & CharType.IsUpper) == CharType.IsUpper)
                    {
                        var p = ptr - 1;
                        if ((_startPtr == p) || ((_startPtr < p) && (*(_CTM + *p) & CharType.IsWhiteSpace) == CharType.IsWhiteSpace))
                        {
                            p = ptr + 1;
                            if ((p == _endPtr) || ((p < _endPtr) && (*(_CTM + *(p + 1)) & CharType.IsWhiteSpace) == CharType.IsWhiteSpace))
                            {
                                if (Xlat.IsDot(*p))
                                    return p;
                            }
                        }
                    }
                    #endregion

                    return ptr;
                }
            }
            return _startPtr - 1;
        }

        private bool IsBetweenLetterOrDigit()
        {
            if (_ptr <= _startPtr)
                return false;

            var ch = *(_ptr - 1);
            var ct = *(_CTM + ch);
            if ((ct & CharType.IsLetter) != CharType.IsLetter && (ct & CharType.IsDigit) != CharType.IsDigit)
            {
                return false;
            }

            var p = _ptr + 1;
            if (_endPtr <= p)
            {
                if (_endPtr < p)
                    return false;
                ch = *p;
                if (ch == '\0')
                    return false;
            }
            else
            {
                ch = *p;
            }
            ct = *(_CTM + ch);
            if ((ct & CharType.IsLetter) != CharType.IsLetter && (ct & CharType.IsDigit) != CharType.IsDigit)
            {
                return false;
            }

            return true;
        }
        private bool IsBetweenDigit()
        {
            if (_ptr <= _startPtr)
                return false;

            var ch = *(_ptr - 1);
            var ct = *(_CTM + ch);
            if ((ct & CharType.IsDigit) != CharType.IsDigit)
            {
                return false;
            }

            var p = _ptr + 1;
            if (_endPtr <= p)
            {
                if (_endPtr < p)
                    return false;
                ch = *p;
                if (ch == '\0')
                    return false;
            }
            else
            {
                ch = *p;
            }
            ct = *(_CTM + ch);
            if ((ct & CharType.IsDigit) != CharType.IsDigit)
            {
                return false;
            }

            return true;
        }
        private bool IsUpperNextChar()
        {
            var p = _ptr + 1;
            char ch;
            if (_endPtr <= p)
            {
                if (_endPtr < p)
                    return false;
                ch = *p;
                if (ch == '\0')
                    return false;
            }
            else
            {
                ch = *p;
            }

            var ct = *(_CTM + ch);
            if ((ct & CharType.IsUpper) != CharType.IsUpper)
            {
                return false;
            }

            return true;
        }

        private bool MergePunctuation(char begining_ch)
        {
            _length = 1;
            _ptr++;
            var whitespace_length = 0;
            for (; _ptr <= _endPtr; _ptr++)
            {
                var ch_next = *_ptr;
                var ct = *(_CTM + ch_next);
                if ((ct & CharType.IsWhiteSpace) == CharType.IsWhiteSpace)
                {
                    whitespace_length++;
                    continue;
                }

                var nct = *(_CCTM + ch_next);
                if ((nct & CRFCharType.InterpreteAsWhitespace) == CRFCharType.InterpreteAsWhitespace)
                {
                    whitespace_length++;
                    continue;
                }

                if (ch_next == begining_ch)
                {
                    _length += whitespace_length + 1;
                    whitespace_length = 0;
                    continue;
                }

                break;
            }
            if (_endPtr < _ptr)
            {
                if ((_length == 1) && (*_endPtr == '\0'))
                    _length = 0;
                return false;
            }
            _ptr -= whitespace_length + 1;

            return true;
        }

        #region ner-model-builder
        private readonly Sentence _buildModelSentence;
        private readonly UrlDetector _urlDetector;
        private readonly List<Buildmodel_word_t> _buildModelWords;
        private BuildModelNerInputType _buildModelNerInputTypeB;
        private BuildModelNerInputType _buildModelNerInputTypeI;

        private Tokenizer(TokenizerConfig4NerModelBuilder config)
        {
            config.UrlDetectorConfig.UrlExtractMode = UrlDetector.UrlExtractModeEnum.Position;

            _urlDetector = new UrlDetector(config.UrlDetectorConfig);
            _buildModelSentence = Sentence.CreateEmpty();
            _words = new List<Word>(DEFAULT_WORDSLIST_CAPACITY);
            _buildModelWords = new List<Buildmodel_word_t>(DEFAULT_WORDSLIST_CAPACITY);
            _particleThatExclusion = config.Model.ParticleThatExclusion;

            _UIM = XlatUnsafe.Inst._UPPER_INVARIANT_MAP;
            _CTM = XlatUnsafe.Inst._CHARTYPE_MAP;
            _CCTM = UnsafeConst.GetInstanceByLanguage(config.LanguageType)._CRF_CHARTYPE_MAP;

            ReAllocWordToUpperBuffer(DEFAULT_WORDTOUPPERBUFFER);

            _posTaggerInputTypeProcessor = DummyPosTaggerInputTypeProcessor.Instance;
            _nerInputTypeProcessor = config.NerInputTypeProcessorFactory.CreateInstance();
        }

        public static Tokenizer Create4NerModelBuilder(TokenizerConfig4NerModelBuilder config)
        {
            var tokenizer = new Tokenizer(config);
            return tokenizer;
        }

        public List<Buildmodel_word_t> Run4ModelBuilder(
            string partOfSentText,
            bool isLastPartOfSentText,
            NerOutputType nerOutputType,
            bool prevPartOfSentTextSameNerOutputType)
        {
            _buildModelWords.Clear();
            if (prevPartOfSentTextSameNerOutputType)
            {
                _buildModelNerInputTypeI = nerOutputType.ToBuildModelNerInputTypeI();
                _buildModelNerInputTypeB = _buildModelNerInputTypeI;
            }
            else
            {
                _buildModelNerInputTypeI = nerOutputType.ToBuildModelNerInputTypeI();
                _buildModelNerInputTypeB = nerOutputType.ToBuildModelNerInputTypeB();
            }

            _outerProcessSentCallbackDelegate = ProcessSentCallbackModelBuilder;

            fixed (char* _base = partOfSentText)
            {
                _BASE = _base;
                _dontSkipNonLetterAndNonDigitToTheEnd = !isLastPartOfSentText;

                var urls = _urlDetector.AllocateUrls(partOfSentText);
                _buildModelSentence.Set4ModelBuilder(0, partOfSentText.Length, (0 < urls.Count) ? urls : null);

                SentSplitterProcessSentCallback(_buildModelSentence);

                _dontSkipNonLetterAndNonDigitToTheEnd = false;
            }

            _outerProcessSentCallbackDelegate = null;

            return _buildModelWords;
        }

        private void ProcessSentCallbackModelBuilder(List<Word> words)
        {
            var len = words.Count;
            if (0 < len)
            {
                _buildModelWords.Add(new Buildmodel_word_t() { word = words[0], buildModelNerInputType = _buildModelNerInputTypeB });
                for (int i = 1; i < len; i++)
                {
                    _buildModelWords.Add(new Buildmodel_word_t() { word = words[i], buildModelNerInputType = _buildModelNerInputTypeI });
                }
            }
        }
        #endregion
    }
}
