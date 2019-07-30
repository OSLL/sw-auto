using System;
using System.Collections.Generic;
using System.Linq;

using LangAnalyzer.Core;

namespace LangAnalyzer.Morphology
{
    /// <summary>
    /// Морфо-модель
    /// </summary>
    internal sealed class MorphoModel : MorphoModelBase, IMorphoModel
    {
        /// словарь слов
        private readonly ITreeDictionary _TreeDictionary;

        public MorphoModel(MorphoModelConfig config) : base(config)
        {
            switch (config.TreeDictionaryType)
            {
                case MorphoModelConfig.TreeDictionaryTypeEnum.Classic:
                    _TreeDictionary = new TreeDictionaryClassic();
                    break;

                case MorphoModelConfig.TreeDictionaryTypeEnum.FastMemPlenty:
                    _TreeDictionary = new TreeDictionaryFastMemPlenty();
                    break;

                default:
                    throw (new ArgumentException(config.TreeDictionaryType.ToString()));
            }

            Initialization(config);
        }
        public void Dispose()
        {
        }

        #region Loading model
        /// морфо-типы
        private Dictionary<string, MorphoType> _MorphoTypesDictionary;
        /// список допустимых комбинаций морфо-аттрибутов в группах 
        private MorphoAttributeList _MorphoAttributeList;
        private PartOfSpeechList _PartOfSpeechList;
        private Dictionary<PartOfSpeechEnum, string> _PartOfSpeechStringDictionary;
        private Action<string, string> _ModelLoadingErrorCallback;

        private struct MorphoTypePair
        {
            public string Name;
            public MorphoType MorphoType;
        }

        /// инициализация морфо-модели
        /// информация для инициализации морфо-модели
        private void Initialization(MorphoModelConfig config)
        {
            #region init fields
            base.Initialization();
            if (config.ModelLoadingErrorCallback == null)
            {
                _ModelLoadingErrorCallback = (s1, s2) => { };
            }
            else
            {
                _ModelLoadingErrorCallback = config.ModelLoadingErrorCallback;
            }
            _MorphoTypesDictionary = new Dictionary<string, MorphoType>();
            _MorphoAttributeList = new MorphoAttributeList();
            _PartOfSpeechList = new PartOfSpeechList();
            _PartOfSpeechStringDictionary = Enum.GetValues(typeof(PartOfSpeechEnum))
                                            .Cast<PartOfSpeechEnum>()
                                            .ToDictionary(pos => pos, pos => pos.ToString());
            #endregion

            foreach (var morphoTypesFilename in config.MorphoTypesFilenames)
            {
                var filename = GetFullFilename(config.BaseDirectory, morphoTypesFilename);
                ReadMorphoTypes(filename);
            }

            foreach (var properNamesFilename in config.ProperNamesFilenames)
            {
                var filename = GetFullFilename(config.BaseDirectory, properNamesFilename);
                ReadWords(filename, MorphoAttributeEnum.Proper);
            }

            foreach (var commonFilename in config.CommonFilenames)
            {
                var filename = GetFullFilename(config.BaseDirectory, commonFilename);
                ReadWords(filename, MorphoAttributeEnum.Common);
            }

            #region uninit fields
            base.UnInitialization();
            _ModelLoadingErrorCallback = null;
            _MorphoTypesDictionary = null;
            _MorphoAttributeList = null;
            _PartOfSpeechList = null;
            _PartOfSpeechStringDictionary = null;
            #endregion

            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
        }

        /// получение морфотипа по его имени
        private MorphoType GetMorphoTypeByName(string name)
        {
            if (_MorphoTypesDictionary.TryGetValue(name, out MorphoType value))
                return value;
            return null;
        }

        /// сохранение морфотипа
        private void AddMorphoType2Dictionary(MorphoTypePair? morphoTypePair)
        {
            if (_MorphoTypesDictionary.ContainsKey(morphoTypePair.Value.Name))
            {
                _ModelLoadingErrorCallback("Duplicated morpho-type", morphoTypePair.Value.Name);
            }
            else
            {
                _MorphoTypesDictionary.Add(morphoTypePair.Value.Name, morphoTypePair.Value.MorphoType);
            }
        }

        /// чтение файла морфотипов
        /// path - полный путь к файлу
        private void ReadMorphoTypes(string filename)
        {
            var lines = ReadFile(filename);
            var morphoAttributePairs = new List<MorphoAttributePair>();
            var morphoForms = new List<MorphoForm>();

            var morphoTypePairLast = default(MorphoTypePair?);
            foreach (var line in lines)
            {
                MorphoTypePair? _morphoTypePair = CreateMorphoTypePair(line);
                if (_morphoTypePair != null) /// новый морфо-тип
				{
                    if (morphoTypePairLast != null)
                    {
                        morphoTypePairLast.Value.MorphoType.SetMorphoForms(morphoForms);

                        AddMorphoType2Dictionary(morphoTypePairLast);
                    }
                    morphoForms.Clear();

                    morphoTypePairLast = _morphoTypePair;
                }
                else /// словоформа
                if (morphoTypePairLast != null)
                {
                    try
                    {
                        MorphoForm morphoForm = CreateMorphoForm(morphoTypePairLast.Value.MorphoType, line, morphoAttributePairs);
                        if (morphoForm != null)
                        {
                            morphoForms.Add(morphoForm);
                        }
                        else
                        {
                            _ModelLoadingErrorCallback("Wrong line format", line);
                        }
                    }
                    catch (MorphoFormatException)
                    {
                        _ModelLoadingErrorCallback("Wrong line format", line);
                    }
                }
                else
                {
                    _ModelLoadingErrorCallback("Null MorphoType", line);
                }
            }

            if (morphoTypePairLast != null)
            {
                morphoTypePairLast.Value.MorphoType.SetMorphoForms(morphoForms);

                AddMorphoType2Dictionary(morphoTypePairLast);
            }
        }

        /// чтение файла со словами
        /// path - полный путь к файлу
        /// nounType - тип существительного
        private void ReadWords(string filename, MorphoAttributeEnum nounType)
        {
            var lines = ReadFile(filename);

            foreach (var line in lines)
            {
                var array = line.Split(WORDS_DICTIONARY_SEPARATOR, StringSplitOptions.RemoveEmptyEntries);
                if (array.Length != 3)
                {
                    _ModelLoadingErrorCallback("Wrong line format", line);
                    continue;
                }

                MorphoType morphoType = GetMorphoTypeByName(array[1]);
                if (morphoType == null)
                {
                    _ModelLoadingErrorCallback("Unknown morpho-type", line);
                }
                else
                if (array[2] != _PartOfSpeechStringDictionary[morphoType.PartOfSpeech])
                {
                    _ModelLoadingErrorCallback("Wrong part-of-speech", line);
                }
                else
                {
                    var word = array[0];

                    var _nounType = default(MorphoAttributePair?);
                    if ((morphoType.MorphoAttributeGroup & MorphoAttributeGroupEnum.NounType) == MorphoAttributeGroupEnum.NounType)
                    {
                        _nounType = _MorphoAttributeList.GetMorphoAttributePair(MorphoAttributeGroupEnum.NounType, nounType);
                    }
                    _TreeDictionary.AddWord(word, morphoType, _nounType);
                }
            }
        }

        /// создание морфоформы из строки
        private MorphoForm CreateMorphoForm(MorphoType morphoType, string line, List<MorphoAttributePair> morphoAttributePairs)
        {
            int index = line.IndexOf(':');
            if (index < 0)
                throw (new MorphoFormatException());

            var ending = StringsHelper.ToLowerInvariant(line.Substring(0, index).Trim());
            if (ending == EMPTY_ENDING)
                ending = string.Empty;

            morphoAttributePairs.Clear();
            var attributes = line.Substring(index + 1).Split(MORPHO_ATTRIBUTE_SEPARATOR, StringSplitOptions.RemoveEmptyEntries);
            foreach (var attribute in attributes)
            {
                var attr = attribute.Trim();
                if (!string.IsNullOrEmpty(attr))
                {
                    if (Enum.TryParse(attr, true, out MorphoAttributeEnum morphoAttribute))
                    {
                        var map = _MorphoAttributeList.TryGetMorphoAttributePair(morphoType.MorphoAttributeGroup, morphoAttribute);
                        if (map.HasValue)
                        {
                            morphoAttributePairs.Add(map.Value);
                        }
#if DEBUG
                        //TOO MANY ERRORS AFTER last (2016.12.28) getting morpho-dcitionaries from 'LangAnalyzer-[ilook]'
                        else
                        {
                            _ModelLoadingErrorCallback( "Error in morpho-attribute: '" + attr + '\'', line );
                        }
#endif
                    }
                    else
                    {
                        _ModelLoadingErrorCallback("Unknown morpho-attribute: '" + attr + '\'', line);
                    }
                }
            }
            var morphoForm = new MorphoForm(ending, morphoAttributePairs);
            return (morphoForm);
        }

        /// создание морфотипа из строки
        private MorphoTypePair? CreateMorphoTypePair(string line)
        {
            var m = MORPHOTYPE_PREFIX_REGEXP.Match(line);
            if (m == null || m.Groups.Count < 3)
            {
                return (null);
            }

            string prefix = m.Groups[1].Value;
            string pos = m.Groups[2].Value;
            string name = line.Substring(prefix.Length);

            var partOfSpeech = default(PartOfSpeechEnum);
            if (Enum.TryParse(pos, true, out partOfSpeech))
            {
                var morphoType = new MorphoType(_PartOfSpeechList.GetPartOfSpeech(partOfSpeech));
                var morphoTypePair = new MorphoTypePair()
                {
                    Name = name,
                    MorphoType = morphoType,
                };
                return (morphoTypePair);
            }
            else
            {
                _ModelLoadingErrorCallback("Unknown part-of-speech: '" + pos + '\'', line);
            }
            return (null);
        }
        #endregion

        public bool GetWordFormMorphologies(string wordUpper, List<WordFormMorphology> result, WordFormMorphologyModeEnum wordFormMorphologyMode)
        {
            return _TreeDictionary.GetWordFormMorphologies(wordUpper, result, wordFormMorphologyMode);
        }
        unsafe public bool GetWordFormMorphologies(char* wordUpper, List<WordFormMorphology> result, WordFormMorphologyModeEnum wordFormMorphologyMode)
        {
            return _TreeDictionary.GetWordFormMorphologies(wordUpper, result, wordFormMorphologyMode);
        }
        public bool GetWordForms(string wordUpper, List<WordForm> result)
        {
            return _TreeDictionary.GetWordForms(wordUpper, result);
        }
    }
}