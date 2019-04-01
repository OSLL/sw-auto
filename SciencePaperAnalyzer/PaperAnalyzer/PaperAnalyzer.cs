using LangAnalyzer.Morphology;
using LangAnalyzer.Postagger;
using LangAnalyzer.SentenceSplitter;
using LangAnalyzer.Tokenizing;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using TestResults.Presentation;
using static LangAnalyzer.Morphology.MorphoModelConfig;
using SentenceHtml = TestResults.Presentation.Sentence;
using WordHtml = TestResults.Presentation.Word;
using Word = LangAnalyzer.Tokenizing.Word;

namespace PaperAnalyzer
{
    /// <summary>
    /// Analyses paper's text
    /// </summary>
    public sealed class PaperAnalyzer
    {
        #region ctor (Lazy singleton implementation)
        /// <summary>
        /// Lazy singleton implementation
        /// </summary>
        private static readonly Lazy<PaperAnalyzer> lazy = new Lazy<PaperAnalyzer>(() => new PaperAnalyzer());

        public static PaperAnalyzer Instance { get { return lazy.Value; } }

        private PaperAnalyzer()
        {
            Environment = AnalyzerEnvironment.Create();
        }
        #endregion

        private static AnalyzerEnvironment Environment { get; set; }

        #region Analyzer Environment
        /// <summary>
        /// Analyzer Environment
        /// </summary>
        private sealed class AnalyzerEnvironment : IDisposable
        {
            private AnalyzerEnvironment()
            {
            }

            public void Dispose()
            {
                if (Processor != null)
                {
                    Processor.Dispose();
                    Processor = null;
                }

                if (MorphoModel != null)
                {
                    MorphoModel.Dispose();
                    MorphoModel = null;
                }

                if (MorphoAmbiguityResolverModel != null)
                {
                    MorphoAmbiguityResolverModel.Dispose();
                    MorphoAmbiguityResolverModel = null;
                }
            }

            private MorphoAmbiguityResolverModel MorphoAmbiguityResolverModel
            {
                get;
                set;
            }

            private IMorphoModel MorphoModel
            {
                get;
                set;
            }

            public PosTaggerProcessor Processor
            {
                get;
                private set;
            }

            public static PosTaggerProcessorConfig CreatePosTaggerProcessorConfig()
            {
                var sentSplitterConfig = new SentSplitterConfig(Config.SENT_SPLITTER_RESOURCES_XML_FILENAME,
                                                                 Config.URL_DETECTOR_RESOURCES_XML_FILENAME);
                var config = new PosTaggerProcessorConfig(Config.TOKENIZER_RESOURCES_XML_FILENAME,
                    Config.POSTAGGER_RESOURCES_XML_FILENAME,
                    LanguageTypeEnum.Ru,
                    sentSplitterConfig)
                {
                    ModelFilename = Config.POSTAGGER_MODEL_FILENAME,
                    TemplateFilename = Config.POSTAGGER_TEMPLATE_FILENAME,
                };

                return config;
            }

            private static MorphoModelConfig CreateMorphoModelConfig()
            {
                var config = new MorphoModelConfig()
                {
                    TreeDictionaryType = TreeDictionaryTypeEnum.Native,
                    BaseDirectory = Config.MORPHO_BASE_DIRECTORY,
                    MorphoTypesFilenames = Config.MORPHO_MORPHOTYPES_FILENAMES,
                    ProperNamesFilenames = Config.MORPHO_PROPERNAMES_FILENAMES,
                    CommonFilenames = Config.MORPHO_COMMON_FILENAMES,
                    ModelLoadingErrorCallback = (s1, s2) => { }
                };

                return config;
            }

            private static MorphoAmbiguityResolverModel CreateMorphoAmbiguityResolverModel()
            {
                var config = new MorphoAmbiguityResolverConfig()
                {
                    ModelFilename = Config.MORPHO_AMBIGUITY_MODEL_FILENAME,
                    TemplateFilename5g = Config.MORPHO_AMBIGUITY_TEMPLATE_FILENAME_5G,
                    TemplateFilename3g = Config.MORPHO_AMBIGUITY_TEMPLATE_FILENAME_3G,
                };

                var model = new MorphoAmbiguityResolverModel(config);
                return model;
            }

            public static AnalyzerEnvironment Create()
            {
                var morphoAmbiguityModel = CreateMorphoAmbiguityResolverModel();
                var morphoModelConfig = CreateMorphoModelConfig();
                var morphoModel = MorphoModelFactory.Create(morphoModelConfig);
                var config = CreatePosTaggerProcessorConfig();

                var posTaggerProcessor = new PosTaggerProcessor(config, morphoModel, morphoAmbiguityModel);

                var environment = new AnalyzerEnvironment()
                {
                    MorphoAmbiguityResolverModel = morphoAmbiguityModel,
                    MorphoModel = morphoModel,
                    Processor = posTaggerProcessor,
                };

                return environment;
            }
        }
        #endregion

        public string ProcessText(string text, string titlesString, string paperName, string refsName)
        {
            try
            {
                if (string.IsNullOrEmpty(refsName))
                    refsName = "Список литературы";
                if (string.IsNullOrEmpty(paperName))
                    paperName = "";
                if (string.IsNullOrEmpty(titlesString))
                    titlesString = "";
                var referencesIndex = text.IndexOf(refsName, StringComparison.InvariantCultureIgnoreCase);
                string referencesSection;
                var refSection = new Section
                {
                    Type = SectionType.ReferencesList
                };
                var refNameSection = new Section()
                {
                    Type = SectionType.SectionTitle
                };
                var refNameSentence = new SentenceHtml(SentenceType.Reference)
                {
                    Original = refsName
                };
                refNameSection.Sentences.Add(refNameSentence);

                if (referencesIndex != -1)
                {
                    referencesSection = text.Substring(referencesIndex);
                    text = text.Remove(referencesIndex);

                    referencesSection = referencesSection.Replace(refsName, "").Trim();
                    var tstRefs = referencesSection.Split("\n");

                    var references = new List<string>();

                    for(int i = 0; i < tstRefs.Length; i++)
                    {
                        if (tstRefs[i].Trim().StartsWith($"{references.Count + 1}."))
                        {
                            references.Add(tstRefs[i].Trim());
                        }
                        else
                        {
                            if (references.Count != 0)
                            {
                                var last = references.Last();
                                references.RemoveAt(references.Count - 1);
                                references.Add(last + tstRefs[i].Trim());
                            }
                        }
                    }

                    foreach(var reference in references)
                    {
                        var refSentence = new SentenceHtml(SentenceType.Reference)
                        {
                            Original = reference
                        };
                        refSection.Sentences.Add(refSentence);
                    }
                }

                text = text.Replace("\n", "");
                //var paperName = "АВТОМАТИЗАЦИЯ ПРОЦЕССА ПРОВЕРКИ ТЕКСТА НА СООТВЕТСТВИЕ НАУЧНОМУ СТИЛЮ";
                //var refsName = "Список использованных источников";

                var titles = new List<string>();
                titles = titlesString.Split("\n").Select(x => x.Trim()).ToList();
                //{
                //    "Проблема и её актуальность",
                //    "Обзор предметной области",
                //    "Выбор метода решения",
                //    "Описание метода решения",
                //    "Исследование решения",
                //    "Результаты исследования",
                //    "Заключение"
                //};

                titles.Add(paperName);

                var titleIndex = text.IndexOf(paperName, StringComparison.InvariantCultureIgnoreCase);
                if (titleIndex != -1)
                    text = text.Substring(titleIndex);            

                var result = Environment.Processor.RunFullAnalysis(text, true, true, true, true);

                var dictionary = new Dictionary<string, int>();
                var stopDictionary = new Dictionary<string, int>();
                var stopPartsOfSpeech = new List<PartOfSpeechEnum>
                {
                    PartOfSpeechEnum.Article,
                    PartOfSpeechEnum.Conjunction,
                    PartOfSpeechEnum.Interjection,
                    PartOfSpeechEnum.Numeral,
                    PartOfSpeechEnum.Other,
                    PartOfSpeechEnum.Particle,
                    PartOfSpeechEnum.Predicate,
                    PartOfSpeechEnum.Preposition,
                    PartOfSpeechEnum.Pronoun
                };

                var pronounsCount = 0;

                var newResult = new List<Word[]>();

                for (int i = 0; i < result.Count; i++)
                {
                    var sentence = result[i];
                    var tmpSentence = new List<Word>();

                    bool upperCaseStreak = false;
                    foreach (var word in sentence)
                    {
                        if (word.posTaggerExtraWordType == PosTaggerExtraWordType.Punctuation || word.morphology.PartOfSpeech == PartOfSpeechEnum.Numeral)
                        {
                            tmpSentence.Add(word);
                            continue;
                        }

                        if (tmpSentence.Count == 0)
                        {
                            tmpSentence.Add(word);
                            if (word.valueOriginal == word.valueUpper)
                                upperCaseStreak = true;
                            continue;
                        }

                        if (word.valueOriginal == word.valueUpper)
                        {
                            if (upperCaseStreak == true)
                            {
                                tmpSentence.Add(word);
                                continue;
                            }
                            else
                            {
                                upperCaseStreak = true;
                                var newSentence = tmpSentence.ConvertAll(x => x);
                                tmpSentence.Clear();
                                tmpSentence.Add(word);
                                newResult.Add(newSentence.ToArray());
                                continue;
                            }
                        }
                        else
                        {
                            if (upperCaseStreak == true)
                                upperCaseStreak = false;
                            if (word.valueOriginal[0] == word.valueUpper[0]
                                && (word.morphology.PartOfSpeech == PartOfSpeechEnum.Noun && word.morphology.MorphoAttribute == MorphoAttributeEnum.Common
                                    || word.morphology.PartOfSpeech == PartOfSpeechEnum.Adjective))
                            {
                                var newSentence = tmpSentence.ConvertAll(x => x);
                                tmpSentence.Clear();
                                tmpSentence.Add(word);
                                newResult.Add(newSentence.ToArray());
                                continue;
                            }
                            else
                            {
                                tmpSentence.Add(word);
                                continue;
                            }
                        }
                    }
                    newResult.Add(tmpSentence.ToArray());
                }

                var titlesTest = newResult.Where(x => x.Last().posTaggerOutputType != PosTaggerOutputType.Punctuation).ToList();

                var sections = new List<Section>();

                var section = new Section();
                foreach (var r in newResult)
                {
                    var sentence = new SentenceHtml(SentenceType.Basic, r.Select(x => new WordHtml(x.valueOriginal, x.posTaggerOutputType)));

                    if (titles.Contains(sentence.ToStringVersion()))
                    {
                        if (section.Sentences.Count() > 0)
                        {
                            sections.Add(section);
                            section = new Section();
                        }
                        section.Type = sections.Count() == 0 ? SectionType.PaperTitle : SectionType.SectionTitle;
                        section.Sentences.Add(sentence);
                        sections.Add(section);
                        section = new Section();
                        continue;
                    }

                    section.Type = SectionType.Text;
                    section.Sentences.Add(sentence);

                    if (r == newResult.Last())
                        sections.Add(section);


                    foreach (var word in r)
                    {
                        var normalForm = word.morphology.NormalForm;
                        if (string.IsNullOrEmpty(normalForm))
                            continue;

                        if (word.morphology.PartOfSpeech == PartOfSpeechEnum.Pronoun)
                            pronounsCount++;

                        if (stopPartsOfSpeech.Contains(word.morphology.PartOfSpeech))
                        {
                            if (stopDictionary.ContainsKey(normalForm))
                                stopDictionary[normalForm]++;
                            else
                                stopDictionary.Add(normalForm, 1);
                        }
                        else
                        {
                            if (dictionary.ContainsKey(normalForm))
                                dictionary[normalForm]++;
                            else
                                dictionary.Add(normalForm, 1);
                        }
                    }
                }

                if (refSection.Sentences.Count() > 0)
                {
                    sections.Add(refNameSection);
                    sections.Add(refSection);
                }

                var top10 = dictionary.OrderByDescending(x => x.Value).Take(10);

                var stopWordCount = stopDictionary.Values.Sum();
                var wordCount = dictionary.Values.Sum() + stopWordCount;
                var keyWordsCount = top10.Take(2).Sum(x => x.Value);

                foreach (var word in top10)
                {
                    Console.WriteLine($"{word.Key} : {word.Value}");
                }


                var waterLvl = stopWordCount / (double)wordCount * 100;
                var keyWordsLvl = keyWordsCount / (double)wordCount * 100;
                var zipfLvl = GetZipf(dictionary);

                var htmlText = string.Join("", sections.Select(x => x.ToStringVersion()));

                var testResult = new
                {
                    waterLvl,
                    keyWordsLvl,
                    zipfLvl,
                    pronounsCount,
                    htmlText
                };

                var jsonObject = JsonConvert.SerializeObject(testResult);

                return jsonObject;
            }
            catch (Exception)
            {
                var testResult = new
                {
                    waterLvl = "что-то пошло не так",
                    keyWordsLvl = "что-то пошло не так",
                    zipfLvl = "что-то пошло не так",
                    pronounsCount = "что-то пошло не так",
                    htmlText = "что-то пошло не так"
                };

                var jsonObject = JsonConvert.SerializeObject(testResult);

                return jsonObject;
            }
            
        }

        private static double GetZipf(Dictionary<string, int> dictionary)
        {
            var wordsForCalculating = dictionary.OrderByDescending(x => x.Value).Where(x => x.Value >= 5).ToList();
            var idealZipf = new List<double>();
            for (int i = 1; i <= wordsForCalculating.Count; i++)
                idealZipf.Add(wordsForCalculating[0].Value / (double)i);

            var deviation = .0;

            for (int i = 0; i < idealZipf.Count; i++)
                deviation += Math.Pow(idealZipf[i] - wordsForCalculating[i].Value, 2);

            return Math.Sqrt(deviation / idealZipf.Count);
        }
    }
}
