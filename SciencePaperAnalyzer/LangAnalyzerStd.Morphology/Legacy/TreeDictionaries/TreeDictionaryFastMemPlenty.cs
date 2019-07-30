using System;
using System.Collections.Generic;
using System.Linq;

using LangAnalyzerStd.Core;

namespace LangAnalyzerStd.Morphology
{
    /// <summary>
    /// Словарь-дерево для слов
    /// </summary>
    internal sealed class TreeDictionaryFastMemPlenty : ITreeDictionary
    {
        /// <summary>
        /// 
        /// </summary>
        private struct Pair
        {
            public Pair(BaseMorphoForm baseMorphoForm, MorphoAttributeEnum morphoAttribute)
            {
                BaseMorphoForm = baseMorphoForm;
                MorphoAttribute = morphoAttribute;
            }

            public BaseMorphoForm BaseMorphoForm;
            public MorphoAttributeEnum MorphoAttribute;
        }

        /// слот для дочерних слов
        private readonly Dictionary<char, TreeDictionaryFastMemPlenty> _Slots;
        /// коллекция информаций о формах слова
        private Dictionary<string, Pair[]> _EndingDictionary;

        public TreeDictionaryFastMemPlenty()
        {
            _Slots = new Dictionary<char, TreeDictionaryFastMemPlenty>();
        }

        #region Append word
        /// добавление слова и всех его форм в словарь
	    /// word - слово
	    /// pMorphoType - морфотип
	    /// nounType - тип сущетсвительного
        unsafe public void AddWord(string word, MorphoType morphoType, MorphoAttributePair? nounType)
        {
            if (morphoType.MorphoForms.Length != 0)
            {
                var len = word.Length - morphoType.MorphoForms[0].Ending.Length;
                var _base = (0 <= len) ? word.Substring(0, len) : word;
                var baseMorphoForm = new BaseMorphoForm(_base, morphoType, nounType);
                var _baseUpper = StringsHelper.ToUpperInvariant(_base);
                fixed (char* baseUpper_ptr = _baseUpper)
                {
                    AddWordPart(baseUpper_ptr, baseMorphoForm);
                }
            }
        }

        /// добавление части слова
        /// wordPart - оставшася часть слова
        /// pBase - базовая форма
        unsafe private void AddWordPart(char* wordPart, BaseMorphoForm baseMorphoForm)
        {
            var first_char = *wordPart;
            if (first_char == '\0')
            /// сохранение характеристик
            {
                var tuples = from morphoForm in baseMorphoForm.MorphoForms
                             select new
                                {
                                    ending = morphoForm.EndingUpper,
                                    baseMorphoForm,
                                    morphoForm,
                                };
                var dict = new Dictionary<string, LinkedList<Pair>>();
                foreach (var t in tuples)
                {
                    if (!dict.TryGetValue(t.ending, out LinkedList<Pair> pairs))
                    {
                        pairs = new LinkedList<Pair>();
                        dict.Add(t.ending, pairs);
                    }
                    pairs.AddLast(new Pair(t.baseMorphoForm, MorphoAttributePair.GetMorphoAttribute(t.baseMorphoForm, t.morphoForm)));
                }

                if (_EndingDictionary == null)
                {
                    _EndingDictionary = new Dictionary<string, Pair[]>();
                }
                foreach (var p in dict)
                {
                    if (!_EndingDictionary.TryGetValue(p.Key, out Pair[] pairs))
                    {
                        _EndingDictionary.Add(p.Key, p.Value.ToArray());
                    }
                    else
                    {
                        _EndingDictionary[p.Key] = pairs.Concat(p.Value).ToArray();
                    }
                }
            }
            else
            {
                if (!_Slots.TryGetValue(first_char, out TreeDictionaryFastMemPlenty value))
                {
                    /// добавление новой буквы
                    value = new TreeDictionaryFastMemPlenty();
                    _Slots.Add(first_char, value);
                }
                value.AddWordPart(wordPart + 1, baseMorphoForm);
            }
        }
        #endregion

        /// получение морфологических свойств слова
	    /// word - слово
	    /// result - коллекция информаций о формах слова	    
        unsafe public bool GetWordFormMorphologies(string wordUpper, List<WordFormMorphology> result, WordFormMorphologyModeEnum wordFormMorphologyMode)
        {
            result.Clear();
            fixed (char* word_ptr = wordUpper)
            {
                FillWordFormMorphologies(word_ptr, result, wordFormMorphologyMode);
            }
            return result.Count != 0;
        }
        unsafe public bool GetWordFormMorphologies(char* wordUpper, List<WordFormMorphology> result, WordFormMorphologyModeEnum wordFormMorphologyMode)
        {
            result.Clear();
            {
                FillWordFormMorphologies(wordUpper, result, wordFormMorphologyMode);
            }
            return result.Count != 0;
        }

        /// получение всех форм слова
        /// word - слово
        /// result - коллекция форм слова	    
        unsafe public bool GetWordForms(string wordUpper, List<WordForm> result)
        {
            result.Clear();
            fixed (char* word_ptr = wordUpper)
            {
                FillWordForms(word_ptr, result);
            }
            return result.Count != 0;
        }

        /// поиск слова в словаре
        /// word - слово
        /// letterIndex - индекс буквы
        /// result - коллекция форм слова      
        unsafe private void FillWordFormMorphologies(char* word, List<WordFormMorphology> result, WordFormMorphologyModeEnum wordFormMorphologyMode)
        {
            FillWordFormMorphologies_Core(word, result, wordFormMorphologyMode);
            var first_char = *word;
            if (first_char != '\0')
            {
                if (_Slots.TryGetValue(first_char, out TreeDictionaryFastMemPlenty value))
                {
                    value.FillWordFormMorphologies(word + 1, result, wordFormMorphologyMode);
                }
            }
        }
        unsafe private void FillWordForms(char* word, List<WordForm> result)
        {
            FillWordForms_Core(word, result);
            var first_char = *word;
            if (first_char != '\0')
            {
                if (_Slots.TryGetValue(first_char, out TreeDictionaryFastMemPlenty value))
                {
                    value.FillWordForms(word + 1, result);
                }
            }
        }

        /// поиск слова в слоте
        /// wordPart - оставшаяся часть слова
        /// pSlot - слот
        /// letterIndex - индекс буквы
        unsafe private void FillWordFormMorphologies_Core(char* wordPart, List<WordFormMorphology> result, WordFormMorphologyModeEnum wordFormMorphologyMode)
        {
            if (_EndingDictionary == null)
                return;

            var ending = new string(wordPart);
            if (_EndingDictionary.TryGetValue(ending, out Pair[] pairs))
            {
                switch (wordFormMorphologyMode)
                {
                    case WordFormMorphologyModeEnum.Default:
                        foreach (var p in pairs)
                        {
                            var wfmi = new WordFormMorphology(p.BaseMorphoForm, p.MorphoAttribute);
                            result.Add(wfmi);
                        }
                        break;

                    case WordFormMorphologyModeEnum.StartsWithLowerLetter:
                        foreach (var p in pairs)
                        {
                            var baseMorphoForm = p.BaseMorphoForm;
                            fixed (char* normalForm_ptr = baseMorphoForm.NormalForm)
                            {
                                var first_char = *normalForm_ptr;
                                if ((first_char != '\0') && *(XlatUnsafe.Inst._UPPER_INVARIANT_MAP + first_char) == first_char)
                                {
                                    continue;
                                }
                            }

                            var wfmi = new WordFormMorphology(baseMorphoForm, p.MorphoAttribute);
                            result.Add(wfmi);
                        }
                        break;

                    case WordFormMorphologyModeEnum.StartsWithUpperLetter:
                        foreach (var p in pairs)
                        {
                            var baseMorphoForm = p.BaseMorphoForm;
                            fixed (char* normalForm_ptr = baseMorphoForm.NormalForm)
                            {
                                var first_char = *normalForm_ptr;
                                if ((first_char != '\0') && *(XlatUnsafe.Inst._UPPER_INVARIANT_MAP + first_char) != first_char)
                                {
                                    continue;
                                }
                            }

                            var wfmi = new WordFormMorphology(baseMorphoForm, p.MorphoAttribute);
                            result.Add(wfmi);
                        }
                        break;

                    case WordFormMorphologyModeEnum.FirstStartsWithUpperAfterLowerLetter:
                        throw new NotImplementedException();

                    case WordFormMorphologyModeEnum.FirstStartsWithLowerAfterUpperLetter:
                        throw new NotImplementedException();
                }
            }
        }
        unsafe private void FillWordForms_Core(char* wordPart, List<WordForm> result)
        {
            if (_EndingDictionary == null)
                return;

            var ending = new string(wordPart);
            if (_EndingDictionary.TryGetValue(ending, out Pair[] pairs))
            {
                foreach (var p in pairs)
                {
                    var partOfSpeech = p.BaseMorphoForm.PartOfSpeech;
                    var _base = p.BaseMorphoForm.Base;
                    foreach (var morphoForm in p.BaseMorphoForm.MorphoForms)
                    {
                        /// получение словоформы
                        var wordForm = _base + morphoForm.Ending;

                        var wf = new WordForm(wordForm, partOfSpeech);
                        result.Add(wf);
                    }
                }
            }
        }

    }
}