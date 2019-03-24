using LangAnalyzer.Morphology;
using LangAnalyzer.Postagger;
using LangAnalyzer.SentenceSplitter;
using LangAnalyzer.Tokenizing;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static LangAnalyzer.Morphology.MorphoModelConfig;

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

        public string ProcessText(string text)
        {
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

            foreach (var r in result)
            {
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
            var paragraphs = text.Split("\n");
            var sb = new StringBuilder();
            foreach(var paragraph in paragraphs)
            {
                if (paragraph.Length > 40)
                    sb.Append($"<p>{paragraph}</p>");
                else
                    sb.Append($"<p style=\"font-weight: bold\">{paragraph}</p>");
            }
            var htmlText = sb.ToString();

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
