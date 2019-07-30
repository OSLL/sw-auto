using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

using LangAnalyzerStd.Core;
using LangAnalyzerStd.Сrfsuite;
using LangAnalyzerStd.Tokenizing;

namespace LangAnalyzerStd.Postagger
{
    /// <summary>
    /// Конвертор в формат CRF
    /// </summary>
    unsafe public sealed class PosTaggerScriber : IDisposable
    {
        private struct PinnedWord
        {
            public char* basePtr;
            public GCHandle gcHandle;

            public int length;
            public PosTaggerInputType posTaggerInputType;
            public PosTaggerExtraWordType posTaggerExtraWordType;
        }

        #region private fields
        private const char VERTICAL_SLASH = '|';
        private const char SLASH = '\\';
        private const char COLON = ':';
        private const char DASH = '-';

        private const int UTF8_BUFFER_SIZE = 1024 * 100;           //100KB
        private const int ATTRIBUTE_MAX_LENGTH = UTF8_BUFFER_SIZE / 4; //25KB
        private const int WORD_MAX_LENGTH = 0x1000;               //4096-chars (4KB) - fusking-enough
        private const int PINNED_WORDS_BUFFER_SIZE = 100;
        private static readonly char[] ALLOWED_COLUMNNAMES = new[] { 'w', 'a', 'b', 'c', 'd', 'e', 'z', 'y' };

        private static readonly Encoding UTF8_ENCODING = Encoding.UTF8;

        private readonly CRFTemplateFile _crfTemplateFile;
        private IntPtr _tagger;
        private readonly GCHandle _UTF8BufferGCHandle;
        private byte* _UTF8BufferPtrBase;
        private readonly GCHandle _attributeBufferGCHandle;
        private char* _attributeBufferPtrBase;
        private char* _attributeBufferPtr;
        private int _pinnedWordsBufferSize;
        private GCHandle _pinnedWordsBufferGCHandle;
        private PinnedWord* _pinnedWordsBufferPtrBase;
        private readonly List<string> _result4ModelBuilder;
        #endregion

        #region ctor dtor dispose
        private PosTaggerScriber(string modelFilename, string templateFilename)
        {
            _crfTemplateFile = CRFTemplateFileLoader.Load(templateFilename, ALLOWED_COLUMNNAMES);

            native.LoadNativeCrfSuite();
            var ptr = Marshal.StringToHGlobalAnsi(modelFilename);
            _tagger = native.crf_tagger_initialize(ptr);
            Marshal.FreeHGlobal(ptr);

            if (_tagger == IntPtr.Zero)
            {
                throw new InvalidDataException("Не удалось открыть CRF-модель.");
            }

            ReAllocPinnedWordsBuffer(PINNED_WORDS_BUFFER_SIZE);

            var utf8Buffer = new byte[UTF8_BUFFER_SIZE];
            _UTF8BufferGCHandle = GCHandle.Alloc(utf8Buffer, GCHandleType.Pinned);
            _UTF8BufferPtrBase = (byte*)_UTF8BufferGCHandle.AddrOfPinnedObject().ToPointer();

            var attributeBuffer = new char[ATTRIBUTE_MAX_LENGTH + 1];
            _attributeBufferGCHandle = GCHandle.Alloc(attributeBuffer, GCHandleType.Pinned);
            _attributeBufferPtrBase = (char*)_attributeBufferGCHandle.AddrOfPinnedObject().ToPointer();
        }
        private PosTaggerScriber(string templateFilename)
        {
            _crfTemplateFile = CRFTemplateFileLoader.Load(templateFilename, ALLOWED_COLUMNNAMES);

            _result4ModelBuilder = new List<string>();

            ReAllocPinnedWordsBuffer(PINNED_WORDS_BUFFER_SIZE);

            var attributeBuffer = new char[ATTRIBUTE_MAX_LENGTH + 1];
            _attributeBufferGCHandle = GCHandle.Alloc(attributeBuffer, GCHandleType.Pinned);
            _attributeBufferPtrBase = (char*)_attributeBufferGCHandle.AddrOfPinnedObject().ToPointer();
        }

        public static PosTaggerScriber Create(string modelFilename, string templateFilename)
        {
            var _PosTaggerCRFSuiteConverter = new PosTaggerScriber(modelFilename, templateFilename);
            return _PosTaggerCRFSuiteConverter;
        }
        public static PosTaggerScriber Create4ModelBuilder(string templateFilename)
        {
            var _PosTaggerCRFSuiteConverter = new PosTaggerScriber(templateFilename);
            return _PosTaggerCRFSuiteConverter;
        }

        private void ReAllocPinnedWordsBuffer(int newBufferSize)
        {
            DisposePinnedWordsBuffer();

            _pinnedWordsBufferSize = newBufferSize;
            var pinnedWordsBuffer = new PinnedWord[_pinnedWordsBufferSize];
            _pinnedWordsBufferGCHandle = GCHandle.Alloc(pinnedWordsBuffer, GCHandleType.Pinned);
            _pinnedWordsBufferPtrBase = (PinnedWord*)_pinnedWordsBufferGCHandle.AddrOfPinnedObject().ToPointer();
        }
        private void DisposePinnedWordsBuffer()
        {
            if (_pinnedWordsBufferPtrBase != null)
            {
                _pinnedWordsBufferGCHandle.Free();
                _pinnedWordsBufferPtrBase = null;
            }
        }

        ~PosTaggerScriber()
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
            if (_tagger != IntPtr.Zero)
            {
                native.crf_tagger_uninitialize(_tagger);
                _tagger = IntPtr.Zero;
            }

            if (_attributeBufferPtrBase != null)
            {
                _attributeBufferGCHandle.Free();
                _attributeBufferPtrBase = null;
            }

            if (_UTF8BufferPtrBase != null)
            {
                _UTF8BufferGCHandle.Free();
                _UTF8BufferPtrBase = null;
            }

            DisposePinnedWordsBuffer();
        }
        #endregion

        public void Run(List<Word> words)
        {
            #region init
            if (!Init(words))
            {
                return;
            }
            var wordsCount = words.Count;
            var wordsCount_Minus1 = wordsCount - 1;
#if DEBUG
                var sb_attr_debug = new StringBuilder();
#endif
            #endregion

            native.crf_tagger_beginAddItemSequence(_tagger);

            #region put attr values to crf
            for (var wordIndex = 0; wordIndex < wordsCount; wordIndex++)
            {
                native.crf_tagger_beginAddItemAttribute(_tagger);

                #region process crf attributes by word
                native.crf_tagger_addItemAttributeNameOnly(_tagger, XlatUnsafe.Inst.PosInputtypeOtherPtrBase);
#if DEBUG
                    sb_attr_debug.Append( PosTaggerInputType.O.ToText() ).Append( '\t' );
#endif

                var ngrams = _crfTemplateFile.GetCRFNgramsWhichCanTemplateBeApplied(wordIndex, wordsCount);
                for (int i = 0, ngramsLength = ngrams.Length; i < ngramsLength; i++)
                {
                    var ngram = ngrams[i];

                    _attributeBufferPtr = ngram.CopyAttributesHeaderChars(_attributeBufferPtrBase);

                    #region build attr values
                    switch (ngram.CRFAttributesLength)
                    {
                        case 1:
                            {
                                AppendAttrValue(wordIndex, ngram.CRFAttribute_0);
                            }
                            break;

                        case 2:
                            {
                                AppendAttrValue(wordIndex, ngram.CRFAttribute_0); *(_attributeBufferPtr++) = VERTICAL_SLASH;
                                AppendAttrValue(wordIndex, ngram.CRFAttribute_1);
                            }
                            break;

                        case 3:
                            {
                                AppendAttrValue(wordIndex, ngram.CRFAttribute_0); *(_attributeBufferPtr++) = VERTICAL_SLASH;
                                AppendAttrValue(wordIndex, ngram.CRFAttribute_1); *(_attributeBufferPtr++) = VERTICAL_SLASH;
                                AppendAttrValue(wordIndex, ngram.CRFAttribute_2);
                            }
                            break;

                        default:
                            {
                                for (var j = 0; j < ngram.CRFAttributesLength; j++)
                                {
                                    var crfAttr = ngram.CRFAttributes[j];
                                    AppendAttrValue(wordIndex, crfAttr); *(_attributeBufferPtr++) = VERTICAL_SLASH;
                                }
                                // Удалить последний '|'
                                _attributeBufferPtr--;
                            }
                            break;
                    }
                    #endregion

                    #region add attr values
                    *(_attributeBufferPtr++) = '\0';
                    var attr_len_with_zero = Math.Min(ATTRIBUTE_MAX_LENGTH, (int)(_attributeBufferPtr - _attributeBufferPtrBase));
                    UTF8_ENCODING.GetBytes(_attributeBufferPtrBase, attr_len_with_zero, _UTF8BufferPtrBase, UTF8_BUFFER_SIZE);
                    native.crf_tagger_addItemAttributeNameOnly(_tagger, _UTF8BufferPtrBase);
                    #endregion
                }

                #region BOS & EOS
                if (wordIndex == 0)
                {
                    native.crf_tagger_addItemAttributeNameOnly(_tagger, XlatUnsafe.Inst.BeginOfSentencePtrBase);
                }
                else
                if (wordIndex == wordsCount_Minus1)
                {
                    native.crf_tagger_addItemAttributeNameOnly(_tagger, XlatUnsafe.Inst.EndOfSentencePtrBase);
                }
                #endregion
                #endregion

                native.crf_tagger_endAddItemAttribute(_tagger);
            }
            #endregion

            native.crf_tagger_endAddItemSequence(_tagger);

            native.crf_tagger_tag(_tagger);

            #region get crf tagging data
            System.Diagnostics.Debug.Assert(native.crf_tagger_getResultLength(_tagger) == wordsCount, "(native.crf_tagger_getResultLength( _Tagger ) != _WordsCount)");
            for (var i = 0; i < wordsCount; i++)
            {
                var ptr = native.crf_tagger_getResultValue(_tagger, (uint)i);

                var value = (byte*)ptr.ToPointer();
                words[i].posTaggerOutputType = PosTaggerExtensions.ToPosTaggerOutputType(value);

                //free pinned-gcHandle
                (_pinnedWordsBufferPtrBase + i)->gcHandle.Free();
            }
            #endregion
        }

        private bool Init(List<Word> words)
        {
            if (words.Count == 0)
            {
                return false;
            }

            var wordsCount = words.Count;

            if (_pinnedWordsBufferSize < wordsCount)
            {
                ReAllocPinnedWordsBuffer(wordsCount);
            }
            for (var i = 0; i < wordsCount; i++)
            {
                var word = words[i];
                var gcHandle = GCHandle.Alloc(word.valueUpper, GCHandleType.Pinned);
                var basePtr = (char*)gcHandle.AddrOfPinnedObject().ToPointer();
                PinnedWord* pw = _pinnedWordsBufferPtrBase + i;
                pw->basePtr = basePtr;
                pw->gcHandle = gcHandle;

                pw->posTaggerInputType = word.posTaggerInputType;
                pw->posTaggerExtraWordType = word.posTaggerExtraWordType;
                pw->length = word.valueUpper.Length;
            }

            return true;
        }

        private void AppendAttrValue(int wordIndex, CRFAttribute crfAttribute)
        {
            switch (crfAttribute.AttributeName)
            {
                //w – слово или словосочетание
                case 'w':
                    {
                        /*
                        символы ':' '\'
                        - их комментировать в поле "w", "\:" и "\\"
                        */
                        var index = wordIndex + crfAttribute.Position;
                        var pw = (_pinnedWordsBufferPtrBase + index);
                        if (pw->posTaggerInputType == PosTaggerInputType.Col)
                        {
                            *(_attributeBufferPtr++) = SLASH;
                            *(_attributeBufferPtr++) = COLON;
                        }
                        else
                        {
                            char* _base = pw->basePtr;
                            switch (*_base)
                            {
                                case SLASH:
                                    *(_attributeBufferPtr++) = SLASH;
                                    *(_attributeBufferPtr++) = SLASH;
                                    break;

                                default:
                                    for (int i = 0, len = Math.Min(WORD_MAX_LENGTH, pw->length); i < len; i++)
                                    {
                                        *(_attributeBufferPtr++) = *(_base + i);
                                    }
                                    break;
                            }
                        }
                    }
                    break;

                //g\a – первая буква с конца
                case 'a':
                    {
                        var index = wordIndex + crfAttribute.Position;
                        var pw = (_pinnedWordsBufferPtrBase + index);

                        if (pw->posTaggerExtraWordType == PosTaggerExtraWordType.Punctuation)
                        {
                            *(_attributeBufferPtr++) = DASH;
                            break;
                        }

                        var len = pw->length - 1;
                        *(_attributeBufferPtr++) = *(pw->basePtr + len);
                    }
                    break;

                //ng\b – две буква с конца
                case 'b':
                    {
                        var index = wordIndex + crfAttribute.Position;
                        var pw = (_pinnedWordsBufferPtrBase + index);

                        if (pw->posTaggerExtraWordType == PosTaggerExtraWordType.Punctuation)
                        {
                            *(_attributeBufferPtr++) = DASH;
                            break;
                        }

                        var len = pw->length - 2;
                        if (0 <= len)
                        {
                            char* _base = pw->basePtr;
                            *(_attributeBufferPtr++) = *(_base + len++);
                            *(_attributeBufferPtr++) = *(_base + len);
                        }
                        else
                        {
                            *(_attributeBufferPtr++) = DASH;
                        }
                    }
                    break;

                //ing\c – три буква с конца
                case 'c':
                    {
                        var index = wordIndex + crfAttribute.Position;
                        var pw = (_pinnedWordsBufferPtrBase + index);

                        if (pw->posTaggerExtraWordType == PosTaggerExtraWordType.Punctuation)
                        {
                            *(_attributeBufferPtr++) = DASH;
                            break;
                        }

                        var len = pw->length - 3;
                        if (0 <= len)
                        {
                            char* _base = pw->basePtr;
                            *(_attributeBufferPtr++) = *(_base + len++);
                            *(_attributeBufferPtr++) = *(_base + len++);
                            *(_attributeBufferPtr++) = *(_base + len);
                        }
                        else
                        {
                            *(_attributeBufferPtr++) = DASH;
                        }
                    }
                    break;

                //ding\d – четыре буква с конца
                case 'd':
                    {
                        var index = wordIndex + crfAttribute.Position;
                        var pw = (_pinnedWordsBufferPtrBase + index);

                        if (pw->posTaggerExtraWordType == PosTaggerExtraWordType.Punctuation)
                        {
                            *(_attributeBufferPtr++) = DASH;
                            break;
                        }

                        var len = pw->length - 4;
                        if (0 <= len)
                        {
                            char* _base = pw->basePtr;
                            *(_attributeBufferPtr++) = *(_base + len++);
                            *(_attributeBufferPtr++) = *(_base + len++);
                            *(_attributeBufferPtr++) = *(_base + len++);
                            *(_attributeBufferPtr++) = *(_base + len);
                        }
                        else
                        {
                            *(_attributeBufferPtr++) = DASH;
                        }
                    }
                    break;

                //nding\e – пять букв с конца
                case 'e':
                    {
                        var index = wordIndex + crfAttribute.Position;
                        var pw = (_pinnedWordsBufferPtrBase + index);

                        if (pw->posTaggerExtraWordType == PosTaggerExtraWordType.Punctuation)
                        {
                            *(_attributeBufferPtr++) = DASH;
                            break;
                        }

                        var len = pw->length - 5;
                        if (0 <= len)
                        {
                            char* _base = pw->basePtr;
                            *(_attributeBufferPtr++) = *(_base + len++);
                            *(_attributeBufferPtr++) = *(_base + len++);
                            *(_attributeBufferPtr++) = *(_base + len++);
                            *(_attributeBufferPtr++) = *(_base + len++);
                            *(_attributeBufferPtr++) = *(_base + len);
                        }
                        else
                        {
                            *(_attributeBufferPtr++) = DASH;
                        }
                    }
                    break;

                //atr\z – список атрибутов (дан ниже)
                case 'z':
                    {
                        var index = wordIndex + crfAttribute.Position;
                        *(_attributeBufferPtr++) = (_pinnedWordsBufferPtrBase + index)->posTaggerInputType.ToCrfChar();
                    }
                    break;

                //y – искомое значение
                case 'y':
                    {
                        *(_attributeBufferPtr++) = 'O';
                    }
                    break;

#if DEBUG
                default: throw (new InvalidDataException( "Invalid column-name: '" + crfAttribute.AttributeName + "'" ));
#endif
            }
        }

        #region model builder
        private void Uninit4ModelBuilder(int wordsCount)
        {
            for (var i = 0; i < wordsCount; i++)
            {
                (_pinnedWordsBufferPtrBase + i)->gcHandle.Free();
            }
        }

        public void WriteCrfAttributesWords4ModelBuilder(TextWriter textWriter, List<Word> buildModel_words)
        {
            if (!Init(buildModel_words))
            {
                return;
            }
            var wordsCount = buildModel_words.Count;

            #region write crf attributes words
            for (int wordIndex = 0; wordIndex < wordsCount; wordIndex++)
            {
                var attrs = GetPosTaggerAttributes4ModelBuilder(
                    wordIndex, wordsCount, buildModel_words[wordIndex].posTaggerOutputType);

                for (int i = 0, attrsLength = attrs.Count; i < attrsLength; i++)
                {
                    textWriter.Write(attrs[i]);
                    textWriter.Write('\t');
                }
                textWriter.Write('\n');
            }
            textWriter.Write('\n');
            #endregion

            Uninit4ModelBuilder(wordsCount);
        }

        unsafe private List<string> GetPosTaggerAttributes4ModelBuilder(int wordIndex, int wordsCount, PosTaggerOutputType posTaggerOutputType)
        {
            var wordsCount_Minus1 = wordsCount - 1;

            _result4ModelBuilder.Clear();

            _result4ModelBuilder.Add(posTaggerOutputType.ToCrfChar().ToString());

            var ngrams = _crfTemplateFile.GetCRFNgramsWhichCanTemplateBeApplied(wordIndex, wordsCount);
            for (int i = 0, ngramsLength = ngrams.Length; i < ngramsLength; i++)
            {
                var ngram = ngrams[i];

                _attributeBufferPtr = ngram.CopyAttributesHeaderChars(_attributeBufferPtrBase);

                #region build attr values
                switch (ngram.CRFAttributesLength)
                {
                    case 1:
                        {
                            AppendAttrValue(wordIndex, ngram.CRFAttribute_0);
                        }
                        break;

                    case 2:
                        {
                            AppendAttrValue(wordIndex, ngram.CRFAttribute_0); *(_attributeBufferPtr++) = VERTICAL_SLASH;
                            AppendAttrValue(wordIndex, ngram.CRFAttribute_1);
                        }
                        break;

                    case 3:
                        {
                            AppendAttrValue(wordIndex, ngram.CRFAttribute_0); *(_attributeBufferPtr++) = VERTICAL_SLASH;
                            AppendAttrValue(wordIndex, ngram.CRFAttribute_1); *(_attributeBufferPtr++) = VERTICAL_SLASH;
                            AppendAttrValue(wordIndex, ngram.CRFAttribute_2);
                        }
                        break;

                    default:
                        {
                            for (var j = 0; j < ngram.CRFAttributesLength; j++)
                            {
                                AppendAttrValue(wordIndex, ngram.CRFAttributes[j]); *(_attributeBufferPtr++) = VERTICAL_SLASH;
                            }
                            // Удалить последний '|'
                            _attributeBufferPtr--;
                        }
                        break;
                }
                #endregion

                var crfValue = new string(_attributeBufferPtrBase, 0, (int)(_attributeBufferPtr - _attributeBufferPtrBase));
                _result4ModelBuilder.Add(crfValue);
            }

            if (wordIndex == 0)
            {
                _result4ModelBuilder.Add(XlatUnsafe.BEGIN_OF_SENTENCE);
            }
            else
            if (wordIndex == wordsCount_Minus1)
            {
                _result4ModelBuilder.Add(XlatUnsafe.END_OF_SENTENCE);
            }

            return _result4ModelBuilder;
        }
        #endregion
    }
}



