using System;
using System.Collections.Generic;

using LangAnalyzer.Core;
using LangAnalyzer.Morphology;
using LangAnalyzer.Tokenizing;

namespace LangAnalyzer.Postagger
{
    /// <summary>
    /// Обработчик именованных сущностей. Обработка с использованием библиотеки CRFSuit 
    /// </summary>
    public sealed class PosTaggerProcessor : IDisposable
    {
        private const int DEFAULT_WORDSLIST_CAPACITY = 1000;
        private readonly Tokenizer _tokenizer;
        private readonly List<Word> _words;
        private readonly PosTaggerScriber _posTaggerScriber;
        private readonly PosTaggerPreMerging _posTaggerPreMerging;
        private readonly PosTaggerMorphoAnalyzer _posTaggerMorphoAnalyzer;
        private readonly Tokenizer.ProcessSentCallbackDelegate _processSentCallback1Delegate;
        private readonly Tokenizer.ProcessSentCallbackDelegate _processSentCallback2Delegate;
        private Tokenizer.ProcessSentCallbackDelegate _outerProcessSentCallbackDelegate;

        public PosTaggerProcessor(PosTaggerProcessorConfig config,
            IMorphoModel morphoModel,
            MorphoAmbiguityResolverModel morphoAmbiguityModel)
        {
            CheckConfig(config, morphoModel, morphoAmbiguityModel);

            _tokenizer = new Tokenizer(config.TokenizerConfig);
            _words = new List<Word>(DEFAULT_WORDSLIST_CAPACITY);
            _posTaggerScriber = PosTaggerScriber.Create(config.ModelFilename, config.TemplateFilename);
            _posTaggerPreMerging = new PosTaggerPreMerging(config.Model);
            _posTaggerMorphoAnalyzer = new PosTaggerMorphoAnalyzer(morphoModel, morphoAmbiguityModel);
            _processSentCallback1Delegate = new Tokenizer.ProcessSentCallbackDelegate(ProcessSentCallback1);
            _processSentCallback2Delegate = new Tokenizer.ProcessSentCallbackDelegate(ProcessSentCallback2);
        }

        public void Dispose()
        {
            _tokenizer.Dispose();
            _posTaggerScriber.Dispose();
            _posTaggerMorphoAnalyzer.Dispose();
        }

        private static void CheckConfig(PosTaggerProcessorConfig config, IMorphoModel morphoModel, MorphoAmbiguityResolverModel morphoAmbiguityModel)
        {
            morphoModel.ThrowIfNull("morphoModel");

            config.ThrowIfNull("config");
            config.Model.ThrowIfNull("Model");
            config.TokenizerConfig.ThrowIfNull("TokenizerConfig");
            config.ModelFilename.ThrowIfNullOrWhiteSpace("ModelFilename");
            config.TemplateFilename.ThrowIfNullOrWhiteSpace("TemplateFilename");

            morphoAmbiguityModel.ThrowIfNull("morphoAmbiguityModel");
        }

        public List<Word> Run(string text, bool splitBySmiles)
        {
            _words.Clear();

            _tokenizer.Run(text, splitBySmiles, _processSentCallback1Delegate);

            return _words;
        }

        private void ProcessSentCallback1(List<Word> words)
        {
            _posTaggerPreMerging.Run(words);

            _posTaggerScriber.Run(words);

#if DEBUG
            _posTaggerMorphoAnalyzer.Run( words, true );
#else
            _posTaggerMorphoAnalyzer.Run(words);
#endif

            _words.AddRange(words);
        }

        public void Run(string text, bool splitBySmiles, Tokenizer.ProcessSentCallbackDelegate processSentCallback)
        {
            _outerProcessSentCallbackDelegate = processSentCallback;

            _tokenizer.Run(text, splitBySmiles, _processSentCallback2Delegate);

            _outerProcessSentCallbackDelegate = null;
        }
        private void ProcessSentCallback2(List<Word> words)
        {
            _posTaggerPreMerging.Run(words);

            _posTaggerScriber.Run(words);

#if DEBUG
            _posTaggerMorphoAnalyzer.Run( words, true );
#else
            _posTaggerMorphoAnalyzer.Run(words);
#endif

            _outerProcessSentCallbackDelegate(words);
        }

        public List<Word[]> RunFullAnalysis(string text, bool splitBySmiles, bool mergeChains, bool processMorphology, bool applyMorphoAmbiguityPreProcess)
        {
            var wordsBySents = new List<Word[]>();

            _tokenizer.Run(text, splitBySmiles, (words) =>
           {
               if (mergeChains)
               {
                   _posTaggerPreMerging.Run(words);
               }

               _posTaggerScriber.Run(words);

               if (processMorphology)
               {
#if DEBUG
                   _posTaggerMorphoAnalyzer.Run( words, applyMorphoAmbiguityPreProcess );
#else
                   _posTaggerMorphoAnalyzer.Run(words);
#endif
               }

               wordsBySents.Add(words.ToArray());
           });

            return wordsBySents;
        }
    }
}
