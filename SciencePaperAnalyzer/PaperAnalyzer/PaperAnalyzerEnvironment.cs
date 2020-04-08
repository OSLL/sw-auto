using LangAnalyzerStd.Morphology;
using LangAnalyzerStd.Postagger;
using LangAnalyzerStd.SentenceSplitter;
using LangAnalyzerStd.Tokenizing;

using static LangAnalyzerStd.Morphology.MorphoModelConfig;

namespace PaperAnalyzer
{
    public class PaperAnalyzerEnvironment : IPaperAnalyzerEnvironment
    {
        public readonly MorphoAmbiguityResolverModel _morphoAmbiguityResolverModel;
        public readonly IMorphoModel _morphoModel;
        public readonly PosTaggerProcessor _processor;

        public MorphoAmbiguityResolverModel MorphoAmbiguityResolverModel => _morphoAmbiguityResolverModel;

        public IMorphoModel MorphoModel => _morphoModel;

        public PosTaggerProcessor Processor => _processor;

        public PaperAnalyzerEnvironment()
        {
            _morphoAmbiguityResolverModel = CreateMorphoAmbiguityResolverModel();
            var morphoModelConfig = CreateMorphoModelConfig();
            _morphoModel = MorphoModelFactory.Create(morphoModelConfig);
            var config = CreatePosTaggerProcessorConfig();
            _processor = new PosTaggerProcessor(config, _morphoModel, _morphoAmbiguityResolverModel);
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
            var config = new MorphoAmbiguityResolverConfig
            {
                ModelFilename = Config.MORPHO_AMBIGUITY_MODEL_FILENAME,
                TemplateFilename5g = Config.MORPHO_AMBIGUITY_TEMPLATE_FILENAME_5G,
                TemplateFilename3g = Config.MORPHO_AMBIGUITY_TEMPLATE_FILENAME_3G,
            };

            var model = new MorphoAmbiguityResolverModel(config);
            return model;
        }

    }
}
