using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

using LangAnalyzer.Core;

namespace LangAnalyzer.Morphology
{
    unsafe internal sealed class MorphoModelNative : MorphoModelBase, IMorphoModel
    {
        private struct MorphoTypeNative_pair_t
        {
            public IntPtr Name;
            public MorphoTypeNative MorphoType;
        }

        private struct CharIntPtr_IEqualityComparer : IEqualityComparer<IntPtr>
        {
            unsafe public bool Equals(IntPtr x, IntPtr y)
            {
                if (x == y)
                    return (true);

                for (char* x_ptr = (char*)x,
                            y_ptr = (char*)y; ; x_ptr++, y_ptr++)
                {
                    var x_ch = *x_ptr;
                    if (x_ch != *y_ptr)
                        return (false);
                    if (x_ch == '\0')
                        return (true);
                }
            }
            unsafe public int GetHashCode(IntPtr obj)
            {
                char* ptr = (char*)obj;
                int n1 = 5381;
                int n2 = 5381;
                int n3;
                while ((n3 = (int)(*(ushort*)ptr)) != 0)
                {
                    n1 = ((n1 << 5) + n1 ^ n3);
                    n2 = ((n2 << 5) + n2 ^ n3);
                    ptr++;
                }
                return (n1 + n2 * 1566083941);
            }
        }

        private sealed class PartOfSpeechToNativeStringMapper : IDisposable
        {
            private Dictionary<PartOfSpeechEnum, IntPtr> _Dictionary;

            public PartOfSpeechToNativeStringMapper()
            {
                _Dictionary = Enum.GetValues(typeof(PartOfSpeechEnum))
                                  .Cast<PartOfSpeechEnum>()
                                  .ToDictionary(pos => pos, pos => Marshal.StringToHGlobalUni(pos.ToString()));
            }
            ~PartOfSpeechToNativeStringMapper()
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
                if (_Dictionary != null)
                {
                    foreach (IntPtr ptr in _Dictionary.Values)
                    {
                        Marshal.FreeHGlobal(ptr);
                    }
                    _Dictionary = null;
                }
            }

            public IntPtr this[PartOfSpeechEnum partOfSpeech]
            {
                get { return _Dictionary[partOfSpeech]; }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private sealed class ModelLoader : IDisposable
        {
            /// <summary>
            /// 
            /// </summary>
            public sealed class EndingsNativeKeeper : IDisposable
            {
                private IntPtr[] _EndingsNative;

                internal EndingsNativeKeeper(IntPtr[] endingsNative)
                {
                    _EndingsNative = endingsNative;
                }
                ~EndingsNativeKeeper()
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
                    if (_EndingsNative != null)
                    {
                        for (int i = 0, len = _EndingsNative.Length; i < len; i++)
                        {
                            Marshal.FreeHGlobal(_EndingsNative[i]);
                        }
                        _EndingsNative = null;
                    }
                }
            }

            #region private fields
            private const int ENDING_BUFFER_SIZE = 256;

            private static readonly CharType* _CHARTYPE_MAP;
            private static readonly char* _UPPER_INVARIANT_MAP;

            private MorphoModelConfig _Config;
            /// морфо-типы
            private Dictionary<IntPtr, IntPtr> _EndingDictionary;
            private Dictionary<IntPtr, MorphoTypeNative> _MorphoTypesDictionary;
            /// список допустимых комбинаций морфо-аттрибутов в группах 
            private MorphoAttributeList _MorphoAttributeList;
            private PartOfSpeechList _PartOfSpeechList;
            private PartOfSpeechToNativeStringMapper _PartOfSpeechToNativeStringMapper;
            private List<MorphoAttributePair> _MorphoAttributePairs_Buffer;
            private Action<string, string> _ModelLoadingErrorCallback;
            private GCHandle _LOWER_INVARIANT_MAP_GCHandle;
            private char* _LOWER_INVARIANT_MAP;
            private char* _ENDING_LOWER_BUFFER;
            private GCHandle _ENDING_LOWER_BUFFER_GCHandle;
            private char* _ENDING_UPPER_BUFFER;
            private GCHandle _ENDING_UPPER_BUFFER_GCHandle;
            private EnumParser<MorphoAttributeEnum> _EnumParserMorphoAttribute;
            private EnumParser<PartOfSpeechEnum> _EnumParserPartOfSpeech;

            //out-of-this-class created field & passed as params of 'Run'-method
            private TreeDictionaryNative _TreeDictionary;
            #endregion

            #region ctor dtor dispose
            static ModelLoader()
            {
                _CHARTYPE_MAP = XlatUnsafe.Inst._CHARTYPE_MAP;
                _UPPER_INVARIANT_MAP = XlatUnsafe.Inst._UPPER_INVARIANT_MAP;
            }

            public ModelLoader(MorphoModelConfig config, TreeDictionaryNative treeDictionary)
            {
                const int ENDING_DICTIONARY_CAPACITY = 350003;
                const int MORPHOTYPES_DICTIONARY_CAPACITY = 4001;
                const int MORPHOATTRIBUTEPAIRS_BUFFER_CAPACITY = 100;

                _TreeDictionary = treeDictionary;
                TreeDictionaryNative.BeginLoad();
                _Config = config;
                _EndingDictionary = new Dictionary<IntPtr, IntPtr>(ENDING_DICTIONARY_CAPACITY, default(CharIntPtr_IEqualityComparer));
                _MorphoTypesDictionary = new Dictionary<IntPtr, MorphoTypeNative>(MORPHOTYPES_DICTIONARY_CAPACITY, default(CharIntPtr_IEqualityComparer));
                _MorphoAttributeList = new MorphoAttributeList();
                _PartOfSpeechList = new PartOfSpeechList();
                _PartOfSpeechToNativeStringMapper = new PartOfSpeechToNativeStringMapper();
                _MorphoAttributePairs_Buffer = new List<MorphoAttributePair>(MORPHOATTRIBUTEPAIRS_BUFFER_CAPACITY);
                _EnumParserMorphoAttribute = new EnumParser<MorphoAttributeEnum>();
                _EnumParserPartOfSpeech = new EnumParser<PartOfSpeechEnum>();
                _EndingDictionary.Add(_EMPTY_STRING, _EMPTY_STRING);
                #region ModelLoadingErrorCallback
                if (config.ModelLoadingErrorCallback == null)
                {
                    _ModelLoadingErrorCallback = (s1, s2) => { };
                }
                else
                {
                    _ModelLoadingErrorCallback = config.ModelLoadingErrorCallback;
                }
                #endregion
                #region LOWER_INVARIANT_MAP
                var lower_invariant_map = Xlat.Create_LOWER_INVARIANT_MAP();
                _LOWER_INVARIANT_MAP_GCHandle = GCHandle.Alloc(lower_invariant_map, GCHandleType.Pinned);
                _LOWER_INVARIANT_MAP = (char*)_LOWER_INVARIANT_MAP_GCHandle.AddrOfPinnedObject().ToPointer();
                #endregion
                #region ENDING_LOWER_BUFFER
                var ending_lower_buffer = new char[ENDING_BUFFER_SIZE];
                _ENDING_LOWER_BUFFER_GCHandle = GCHandle.Alloc(ending_lower_buffer, GCHandleType.Pinned);
                _ENDING_LOWER_BUFFER = (char*)_ENDING_LOWER_BUFFER_GCHandle.AddrOfPinnedObject().ToPointer();
                #endregion
                #region ENDING_UPPER_BUFFER
                var ending_upper_buffer = new char[ENDING_BUFFER_SIZE];
                _ENDING_UPPER_BUFFER_GCHandle = GCHandle.Alloc(ending_upper_buffer, GCHandleType.Pinned);
                _ENDING_UPPER_BUFFER = (char*)_ENDING_UPPER_BUFFER_GCHandle.AddrOfPinnedObject().ToPointer();
                #endregion
            }
            ~ModelLoader()
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
                _TreeDictionary = null;
                TreeDictionaryNative.EndLoad();
                _LOWER_INVARIANT_MAP_GCHandle.Free(); _LOWER_INVARIANT_MAP = null;
                _ENDING_LOWER_BUFFER_GCHandle.Free(); _ENDING_LOWER_BUFFER = null;
                _ENDING_UPPER_BUFFER_GCHandle.Free(); _ENDING_UPPER_BUFFER = null;
                _EndingDictionary = null;
                _ModelLoadingErrorCallback = null;
                _MorphoTypesDictionary = null;
                _MorphoAttributeList.Dispose(); _MorphoAttributeList = null;
                _PartOfSpeechList = null;
                _PartOfSpeechToNativeStringMapper.Dispose(); _PartOfSpeechToNativeStringMapper = null;
                _MorphoAttributePairs_Buffer = null;
                _EnumParserMorphoAttribute = null;
                _EnumParserPartOfSpeech = null;
                _Config = default;
            }
            #endregion

            public EndingsNativeKeeper Run()
            {
                Console.WriteLine("\r\n\r\n");
                var sw = new System.Diagnostics.Stopwatch();
                foreach (var morphoTypesFilename in _Config.MorphoTypesFilenames)
                {
                    var filename = GetFullFilename(_Config.BaseDirectory, morphoTypesFilename);
                    sw.Restart();
                    ReadMorphoTypes(filename);
                    sw.Stop();
                    Console.WriteLine("elapsed: " + sw.Elapsed + ", " + filename);
                }

                foreach (var properNamesFilename in _Config.ProperNamesFilenames)
                {
                    var filename = GetFullFilename(_Config.BaseDirectory, properNamesFilename);
                    sw.Restart();
                    ReadWords(filename, MorphoAttributeEnum.Proper);
                    sw.Stop();
                    Console.WriteLine("elapsed: " + sw.Elapsed + ", " + filename);
                }

                foreach (var commonFilename in _Config.CommonFilenames)
                {
                    var filename = GetFullFilename(_Config.BaseDirectory, commonFilename);
                    sw.Restart();
                    ReadWords(filename, MorphoAttributeEnum.Common);
                    sw.Stop();
                    Console.WriteLine("elapsed: " + sw.Elapsed + ", " + filename);
                }

                sw.Restart();
                _TreeDictionary.Trim();
                sw.Stop();
                Console.WriteLine("elapsed: " + sw.Elapsed + ", _TreeDictionary.Trim();");
                IntPtr[] endingsNative = (_Config.IsPermanentStayInMemoryUseType ? null : _EndingDictionary.Keys.ToArray());

                return (new EndingsNativeKeeper(endingsNative));
            }

            private MorphoTypeNative _MorphoTypeByNameValue;
            /// <summary>
            /// получение морфотипа по его имени
            /// </summary>
            private MorphoTypeNative GetMorphoTypeByName(IntPtr name)
            {
                if (_MorphoTypesDictionary.TryGetValue(name, out _MorphoTypeByNameValue))
                {
                    return (_MorphoTypeByNameValue);
                }
                return (null);
            }

            /// <summary>
            /// сохранение морфотипа
            /// </summary>
            private void AddMorphoType2Dictionary(ref MorphoTypeNative_pair_t morphoTypePair)
            {
                if (_MorphoTypesDictionary.ContainsKey(morphoTypePair.Name))
                {
                    _ModelLoadingErrorCallback("Duplicated morpho-type", StringsHelper.ToString(morphoTypePair.Name)); //throw (new DuplicatedMorphoTypeException());
                }
                else
                {
                    _MorphoTypesDictionary.Add(morphoTypePair.Name, morphoTypePair.MorphoType);
                }
            }

            /// <summary>
            /// чтение файла морфотипов, filename - полный путь к файлу
            /// </summary>
            private void ReadMorphoTypes(string filename)
            {
                const int MORPHOFORMS_CAPACITY = 150;

                var lines = ReadFile(filename);
                var morphoForms = new List<MorphoFormNative>(MORPHOFORMS_CAPACITY);

                var morphoTypePairLast = default(MorphoTypeNative_pair_t?);
                foreach (var line in lines)
                {
                    fixed (char* lineBase = line)
                    {
                        MorphoTypeNative_pair_t? morphoTypePair = CreateMorphoTypePair(lineBase, line.Length);
                        /// новый морфо-тип
                        if (morphoTypePair.HasValue)
                        {
                            if (morphoTypePairLast.HasValue)
                            {
                                var mtp = morphoTypePairLast.Value;
                                mtp.MorphoType.SetMorphoForms(morphoForms);

                                AddMorphoType2Dictionary(ref mtp);
                            }
                            morphoForms.Clear();

                            morphoTypePairLast = morphoTypePair;
                        }
                        else
                        /// словоформа в последнем морфо-типе
                        if (morphoTypePairLast.HasValue)
                        {
                            MorphoFormNative? morphoForm = CreateMorphoForm(morphoTypePairLast.Value.MorphoType, lineBase);
                            if (morphoForm.HasValue)
                            {
                                morphoForms.Add(morphoForm.Value);
                            }
                            else
                            {
                                _ModelLoadingErrorCallback("Wrong line format", line);
                            }
                        }
                        else
                        {
                            _ModelLoadingErrorCallback("Null MorphoType", line);
                        }
                    }
                }
                /// последний морфо-тип
                if (morphoTypePairLast.HasValue)
                {
                    var mtp = morphoTypePairLast.Value;
                    mtp.MorphoType.SetMorphoForms(morphoForms);

                    AddMorphoType2Dictionary(ref mtp);
                }
            }

            /// чтение файла со словами
            /// path - полный путь к файлу
            /// nounType - тип существи тельного
            private void ReadWords(string filename, MorphoAttributeEnum nounType)
            {
                var lines = ReadFile(filename);

                var plw = default(ParsedLineWords_unsafe);

                foreach (var line in lines)
                {
                    fixed (char* lineBase = line)
                    {
                        if (!ParseLineWords(lineBase, ref plw))
                        {
                            _ModelLoadingErrorCallback("Wrong line format", line);
                            continue;
                        }

                        MorphoTypeNative morphoType = GetMorphoTypeByName((IntPtr)plw.MorphoTypeName);
                        if (morphoType == null)
                        {
                            _ModelLoadingErrorCallback("Unknown morpho-type", line);
                            continue;
                        }

                        if (!StringsHelper.IsEqual((IntPtr)plw.PartOfSpeech, _PartOfSpeechToNativeStringMapper[morphoType.PartOfSpeech]))
                        {
                            _ModelLoadingErrorCallback("Wrong part-of-speech", line);
                            continue;
                        }

                        if (morphoType.HasMorphoForms)
                        {
                            var nounTypePair = default(MorphoAttributePair?);
                            if ((morphoType.MorphoAttributeGroup & MorphoAttributeGroupEnum.NounType) == MorphoAttributeGroupEnum.NounType)
                            {
                                nounTypePair = _MorphoAttributeList.GetMorphoAttributePair(MorphoAttributeGroupEnum.NounType, nounType);
                            }

                            #region Allocate native-memory for baseOfWord
                            var len = plw.WordLength - StringsHelper.GetLength(morphoType.FirstEnding);
                            len = ((0 <= len) ? len : plw.WordLength);

                            IntPtr lineBasePtr;
                            if (0 < len)
                            {
                                *(lineBase + len) = '\0';
                                lineBasePtr = new IntPtr(lineBase);

                                if (_EndingDictionary.TryGetValue(lineBasePtr, out IntPtr existsPtr))
                                {
                                    lineBasePtr = existsPtr;
                                }
                                else
                                {
                                    AllocHGlobalAndCopy(lineBase, len, out lineBasePtr);
                                    _EndingDictionary.Add(lineBasePtr, lineBasePtr);
                                }
                            }
                            else
                            {
                                lineBasePtr = _EMPTY_STRING;
                            }
                            #endregion

                            _TreeDictionary.AddWord((char*)lineBasePtr, morphoType, ref nounTypePair);
                        }
                    }
                }
            }

            /// <summary>
            /// создание морфоформы из строки
            /// </summary>
            private MorphoFormNative? CreateMorphoForm(MorphoTypeNative morphoType, char* lineBase)
            {
                #region find index-of-COLON & check on length
                var index = IndexOf(lineBase, COLON);
                if ((index == -1) || (ENDING_BUFFER_SIZE <= index))
                {
                    _ModelLoadingErrorCallback("Index of COLON is undefined or length the line is too long", StringsHelper.ToString(lineBase));
                    return (null);
                }
                #endregion

                #region fill '_ENDING_LOWER_BUFFER'
                var i = 0;
                for (char* ptr = lineBase; i < index; ptr++, i++)
                {
                    var ch = *ptr;
                    if ((_CHARTYPE_MAP[ch] & CharType.IsWhiteSpace) == CharType.IsWhiteSpace)
                        break;
                    _ENDING_LOWER_BUFFER[i] = _LOWER_INVARIANT_MAP[ch];
                }
                _ENDING_LOWER_BUFFER[i] = '\0';
                #endregion

                #region fill '_MorphoAttributePairs_Buffer'
                _MorphoAttributePairs_Buffer.Clear();
                for (char* ptr = lineBase + index + 1; ; ptr++)
                {
                    var ch = *ptr;
                    if (ch == '\0')
                        break;

                    if ((_CHARTYPE_MAP[ch] & CharType.IsLetter) != CharType.IsLetter)
                        continue;

                    var len = 0;
                    for (; ; ptr++)
                    {
                        ch = *ptr;
                        if (ch == '\0')
                            break;
                        var ct = _CHARTYPE_MAP[ch];
                        if ((ct & CharType.IsLetter) != CharType.IsLetter &&
                                (ct & CharType.IsDigit) != CharType.IsDigit)
                        {
                            break;
                        }
                        len++;
                    }
                    if (len != 0)
                    {
                        var morphoAttribute = default(MorphoAttributeEnum);
                        if (_EnumParserMorphoAttribute.TryParse(ptr - len, len, ref morphoAttribute))
                        {
                            var map = _MorphoAttributeList.TryGetMorphoAttributePair(morphoType.MorphoAttributeGroup, morphoAttribute);
                            if (map.HasValue)
                            {
                                _MorphoAttributePairs_Buffer.Add(map.Value);
                            }
#if DEBUG
                            //*
                            //TOO MANY ERRORS AFTER last (2016.12.28) getting morpho-dcitionaries from 'LangAnalyzer-[ilook]'
                            else
                            {
                                var attr = new string( ptr - len, 0, len );
                                _ModelLoadingErrorCallback( "Error in morpho-attribute: '" + attr + '\'', StringsHelper.ToString( lineBase ) );
                            }
                            //*/
#endif
                        }
                        else
                        {
                            var attr = new string(ptr - len, 0, len);
                            _ModelLoadingErrorCallback("Unknown morpho-attribute: '" + attr + '\'', StringsHelper.ToString(lineBase));
                        }
                    }

                    if (ch == '\0')
                        break;
                }
                #endregion

                #region Allocate native-memory for endingOfWord
                //*
                IntPtr endingPtr;
                IntPtr endingUpperPtr;
                if ((i == 1) && (_ENDING_LOWER_BUFFER[0] == UNDERLINE))
                {
                    endingPtr = _EMPTY_STRING;
                    endingUpperPtr = _EMPTY_STRING;
                }
                else
                {
                    #region ending-in-original-case
                    endingPtr = new IntPtr(_ENDING_LOWER_BUFFER);

                    if (_EndingDictionary.TryGetValue(endingPtr, out IntPtr existsPtr))
                    {
                        endingPtr = existsPtr;
                    }
                    else
                    {
                        AllocHGlobalAndCopy(_ENDING_LOWER_BUFFER, index, out endingPtr);
                        _EndingDictionary.Add(endingPtr, endingPtr);
                    }
                    #endregion

                    #region ending-in-upper-case
                    StringsHelper.ToUpperInvariant(_ENDING_LOWER_BUFFER, _ENDING_UPPER_BUFFER);

                    endingUpperPtr = new IntPtr(_ENDING_UPPER_BUFFER);

                    if (_EndingDictionary.TryGetValue(endingUpperPtr, out existsPtr))
                    {
                        endingUpperPtr = existsPtr;
                    }
                    else
                    {
                        AllocHGlobalAndCopy(_ENDING_UPPER_BUFFER, index, out endingUpperPtr);
                        _EndingDictionary.Add(endingUpperPtr, endingUpperPtr);
                    }
                    #endregion
                }
                #endregion

                var morphoForm = new MorphoFormNative((char*)endingPtr, (char*)endingUpperPtr, _MorphoAttributePairs_Buffer);
                return morphoForm;
            }

            /// <summary>
            /// создание морфотипа из строки
            /// </summary>
            private MorphoTypeNative_pair_t? CreateMorphoTypePair(char* lineBase, int lineLength)
            {
                var index1 = IndexOf(lineBase, COMMA);
                if (index1 == -1)
                {
                    return null;
                }
                var index2 = IndexAfter_MORPHO_TYPE(lineBase + index1 + 1);
                if (index2 == -1)
                {
                    return null;
                }

                var partOfSpeech = default(PartOfSpeechEnum);
                if (_EnumParserPartOfSpeech.TryParse(lineBase, index1, ref partOfSpeech))
                {
                    var startIndex = index1 + 1 + index2 + 1;
                    IntPtr namePtr;
                    AllocHGlobalAndCopy(lineBase + startIndex, lineLength - startIndex, out namePtr);

                    var morphoType = new MorphoTypeNative(_PartOfSpeechList.GetPartOfSpeech(partOfSpeech));
                    var morphoTypePair = new MorphoTypeNative_pair_t()
                    {
                        Name = namePtr,
                        MorphoType = morphoType,
                    };
                    return morphoTypePair;
                }
                else
                {
                    var pos = StringsHelper.ToString(lineBase, index1);
                    _ModelLoadingErrorCallback("Unknown part-of-speech: '" + pos + '\'', StringsHelper.ToString(lineBase));
                }
                return null;
            }

            private static int IndexAfter_MORPHO_TYPE(char* ptr)
            {
                var index = IndexOf(ptr, COLON);
                if (index == -1)
                    return -1;

                for (char* p = ptr + index - 1; ptr <= p; p--)
                {
                    if ((_CHARTYPE_MAP[*p] & CharType.IsWhiteSpace) == CharType.IsWhiteSpace)
                        continue;

                    //"MORPHO_TYPE"
                    if (*(p--) != 'E') break;
                    if (*(p--) != 'P') break;
                    if (*(p--) != 'Y') break;
                    if (*(p--) != 'T') break;
                    if (*(p--) != '_') break;
                    if (*(p--) != 'O') break;
                    if (*(p--) != 'H') break;
                    if (*(p--) != 'P') break;
                    if (*(p--) != 'R') break;
                    if (*(p--) != 'O') break;
                    if (*(p--) != 'M') break;
                    if ((_CHARTYPE_MAP[*p] & CharType.IsWhiteSpace) != CharType.IsWhiteSpace) break;

                    return index;
                }

                return -1;
            }

            private static void AllocHGlobalAndCopy(char* source, int sourceLength, out IntPtr dest)
            {
                //alloc with include zero-'\0' end-of-string
                dest = Marshal.AllocHGlobal((sourceLength + 1) * sizeof(char));
                var destPtr = (char*)dest;
                for (; 0 < sourceLength; sourceLength--)
                {
                    *(destPtr++) = *(source++);
                }
                *destPtr = '\0';
            }
        }

        #region private fields
        // empty-native-string
        private static readonly IntPtr _EMPTY_STRING;

        // словарь слов
        private readonly TreeDictionaryNative _TreeDictionary;
        // array of endings-of-word. if not NULL, then for faster call 'Marshal.FreeHGlobal'
        private readonly ModelLoader.EndingsNativeKeeper _EndingsNativeKeeper;
        #endregion

        #region ctor & dispose
        static MorphoModelNative()
        {
            _EMPTY_STRING = Marshal.AllocHGlobal(sizeof(char));
            *((char*)_EMPTY_STRING) = '\0';
        }

        public MorphoModelNative(MorphoModelConfig config) : base(config)
        {
            _TreeDictionary = new TreeDictionaryNative();

            using (var loader = new ModelLoader(config, _TreeDictionary))
            {
                _EndingsNativeKeeper = loader.Run();
            }
        }

        public void Dispose()
        {
            _EndingsNativeKeeper.Dispose();
        }
        #endregion

        public bool GetWordFormMorphologies(string wordUpper, List<WordFormMorphology> result, WordFormMorphologyModeEnum wordFormMorphologyMode)
        {
            return (_TreeDictionary.GetWordFormMorphologies(wordUpper, result, wordFormMorphologyMode));
        }
        public bool GetWordFormMorphologies(char* wordUpper, List<WordFormMorphology> result, WordFormMorphologyModeEnum wordFormMorphologyMode)
        {
            return (_TreeDictionary.GetWordFormMorphologies(wordUpper, result, wordFormMorphologyMode));
        }
        public bool GetWordForms(string wordUpper, List<WordForm> result)
        {
            return (_TreeDictionary.GetWordForms(wordUpper, result));
        }
    }
}