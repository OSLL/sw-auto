using System.Collections.Generic;

using LangAnalyzerStd.Core;

namespace LangAnalyzerStd.Morphology
{
    /// <summary>
    /// Морфо-анализатор
	/// </summary>
	public sealed class MorphoAnalyzer
    {
        #region private fields
        private const int DEFAULT_BUFFER_4_UPPER_SIZE = 256;
        /// морфо-модель
        private readonly IMorphoModel _morphoModel;
        private readonly List<WordFormMorphology> _wordFormMorphologies;
        private readonly List<WordForm> _wordForms;
        private readonly Dictionary<string, PartOfSpeechEnum> _uniqueWordFormsDictionary;
        #endregion

        public char[] Buffer4Upper { get; }

        public MorphoAnalyzer(IMorphoModel model)
        {
            model.ThrowIfNull("model");

            _morphoModel = model;
            _wordFormMorphologies = new List<WordFormMorphology>();
            _wordForms = new List<WordForm>();
            _uniqueWordFormsDictionary = new Dictionary<string, PartOfSpeechEnum>();
            Buffer4Upper = new char[DEFAULT_BUFFER_4_UPPER_SIZE];
        }

        /// получение морфологической информации
        /// words - слова
        public WordMorphology GetWordMorphology(string word)
        {
            var wordUpper = StringsHelper.ToUpperInvariant(word);

            return GetWordMorphology_NoToUpper(wordUpper, WordFormMorphologyModeEnum.Default);
        }
        public WordMorphology GetWordMorphology(string word, WordFormMorphologyModeEnum wordFormMorphologyMode)
        {
            var wordUpper = StringsHelper.ToUpperInvariant(word);

            return GetWordMorphology_NoToUpper(wordUpper, wordFormMorphologyMode);
        }
        public WordMorphology GetWordMorphology_NoToUpper(string wordUpper)
        {
            return GetWordMorphology_NoToUpper(wordUpper, WordFormMorphologyModeEnum.Default);
        }
        public WordMorphology GetWordMorphology_NoToUpper(string wordUpper, WordFormMorphologyModeEnum wordFormMorphologyMode)
        {
            var wordMorphology = new WordMorphology();

            if (_morphoModel.GetWordFormMorphologies(wordUpper, _wordFormMorphologies, wordFormMorphologyMode))
            {
                var len = _wordFormMorphologies.Count;
                switch (len)
                {
                    case 0: break;
                    case 1:
                        wordMorphology.IsSinglePartOfSpeech = true;
                        wordMorphology.PartOfSpeech = _wordFormMorphologies[0].PartOfSpeech;
                        wordMorphology.WordFormMorphologies = _wordFormMorphologies;
                        break;
                    default:
                        for (int i = 0; i < len; i++)
                        {
                            var pos = _wordFormMorphologies[i].PartOfSpeech;
                            if (i == 0)
                                wordMorphology.IsSinglePartOfSpeech = true;
                            else
                                wordMorphology.IsSinglePartOfSpeech &= (wordMorphology.PartOfSpeech == pos);
                            wordMorphology.PartOfSpeech |= pos;
                        }

                        wordMorphology.WordFormMorphologies = _wordFormMorphologies;
                        break;
                }
            }

            return wordMorphology;
        }

        unsafe public WordMorphology GetWordMorphology_4LastValueUpperInNumeralChain(string wordUpper, WordFormMorphologyModeEnum wordFormMorphologyMode)
        {
            fixed (char* wordUpper_ptr = wordUpper)
            {
                return GetWordMorphology_4LastValueUpperInNumeralChain(wordUpper_ptr, wordFormMorphologyMode);
            }
        }
        
        unsafe private WordMorphology GetWordMorphology_4LastValueUpperInNumeralChain(char* wordUpper, WordFormMorphologyModeEnum wordFormMorphologyMode)
        {
            var wordMorphology = new WordMorphology();

            if (_morphoModel.GetWordFormMorphologies(wordUpper, _wordFormMorphologies, wordFormMorphologyMode))
            {
                var len = _wordFormMorphologies.Count;
                switch (len)
                {
                    case 0: break;
                    case 1:
                        wordMorphology.IsSinglePartOfSpeech = true;
                        wordMorphology.PartOfSpeech = _wordFormMorphologies[0].PartOfSpeech;
                        wordMorphology.WordFormMorphologies = _wordFormMorphologies;
                        break;
                    default:
                        for (int i = 0; i < len; i++)
                        {
                            var pos = _wordFormMorphologies[i].PartOfSpeech;
                            if (i == 0)
                                wordMorphology.IsSinglePartOfSpeech = true;
                            else
                                wordMorphology.IsSinglePartOfSpeech &= (wordMorphology.PartOfSpeech == pos);
                            wordMorphology.PartOfSpeech |= pos;
                        }

                        wordMorphology.WordFormMorphologies = _wordFormMorphologies;
                        break;
                }
            }

            return wordMorphology;
        }

        /// получение форм слова
        /// word - слово
        /// pos - часть речи
        public WordForms GetWordForms(string word)
        {
            return GetWordFormsByPartOfSpeech(word, PartOfSpeechEnum.Other);
        }

        public WordForms GetWordForms_NoToUpper(string wordUpper)
        {
            return GetWordFormsByPartOfSpeech_NoToUpper(wordUpper, PartOfSpeechEnum.Other);
        }

        public WordForms GetWordFormsByPartOfSpeech(string word, PartOfSpeechEnum partOfSpeechFilter)
        {
            var result = new WordForms(word);
            var wordUpper = StringsHelper.ToUpperInvariant(word);

            if (_morphoModel.GetWordForms(wordUpper, _wordForms))
            {
                FillUniqueWordFormsDictionary(partOfSpeechFilter);

                _wordForms.Clear();
                foreach (var p in _uniqueWordFormsDictionary)
                {
                    var form = p.Key;
                    var partOfSpeech = p.Value;

                    var wf = new WordForm(form, partOfSpeech);
                    _wordForms.Add(wf);
                }
                result.Forms = _wordForms;
            }

            return result;
        }
        public WordForms GetWordFormsByPartOfSpeech_NoToUpper(string wordUpper, PartOfSpeechEnum partOfSpeechFilter)
        {
            var result = new WordForms(wordUpper);

            if (_morphoModel.GetWordForms(wordUpper, _wordForms))
            {
                FillUniqueWordFormsDictionary(partOfSpeechFilter);

                _wordForms.Clear();
                foreach (var p in _uniqueWordFormsDictionary)
                {
                    var form = p.Key;
                    var partOfSpeech = p.Value;

                    var wf = new WordForm(form, partOfSpeech);
                    _wordForms.Add(wf);
                }
                result.Forms = _wordForms;
            }

            return result;
        }

        /// получение уникальных форм
        /// pForms - все формы
        /// uniqueForms [out] - уникальные формы
        /// pos - часть речи
        /// result - общее число уникальных форм
        private void FillUniqueWordFormsDictionary(PartOfSpeechEnum partOfSpeechFilter)
        {
            _uniqueWordFormsDictionary.Clear();

            for (int i = 0, len = _wordForms.Count; i < len; i++)
            {
                var wordForm = _wordForms[i];
                PartOfSpeechEnum partOfSpeechForm = wordForm.PartOfSpeech;
                if ((partOfSpeechForm & partOfSpeechFilter) != partOfSpeechFilter)
                    continue;

                var wordFormString = wordForm.Form;

                if (_uniqueWordFormsDictionary.TryGetValue(wordFormString, out PartOfSpeechEnum partOfSpeechExists))
                {
                    if ((partOfSpeechExists & partOfSpeechForm) != partOfSpeechForm)
                    {
                        partOfSpeechExists |= partOfSpeechForm;
                        _uniqueWordFormsDictionary[wordFormString] = partOfSpeechExists;
                    }
                }
                else
                {
                    _uniqueWordFormsDictionary.Add(wordFormString, partOfSpeechForm);
                }
            }
        }

    }
}