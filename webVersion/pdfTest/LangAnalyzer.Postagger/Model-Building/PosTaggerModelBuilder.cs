using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using LangAnalyzer.Core;
using LangAnalyzer.Tokenizing;
using LangAnalyzer.Urls;

namespace LangAnalyzer.Postagger
{
    /// <summary>
    /// Обработчик именованных сущностей. Обработка с использованием библиотеки CRFSuit 
    /// </summary>
    public sealed class PosTaggerModelBuilder
    {
        private static readonly char[] SPLIT_CHARS = new[] { '\t' };
        private readonly IPosTaggerInputTypeProcessor _posTaggerInputTypeProcessor;
        private readonly PosTaggerScriber _posTaggerScriber;
        private readonly UrlDetector _urlDetector;
        private readonly List<Word> _words;

        public PosTaggerModelBuilder(string templateFilename,
            LanguageTypeEnum languageType,
            UrlDetectorConfig urlDetectorConfig)
        {
            templateFilename.ThrowIfNullOrWhiteSpace("templateFilename");
            urlDetectorConfig.ThrowIfNull("urlDetectorConfig");

            _posTaggerScriber = PosTaggerScriber.Create4ModelBuilder(templateFilename);
            _posTaggerInputTypeProcessor = CreatePosTaggerInputTypeProcessor(languageType);
            _urlDetector = new UrlDetector(urlDetectorConfig);
            _words = new List<Word>();
        }

        public delegate void ProcessSentCallbackDelegate(int sentNumber);
        public delegate void ProcessErrorSentCallbackDelegate(string line, int sentNumber);
        public delegate void StartBuildCallbackDelegate();

        unsafe public int CreateCrfInputFormatFile(TextWriter textWriter,
            TextReader textReader,
            ProcessSentCallbackDelegate processSentCallback,
            int sentNumberCallbackStep)
        {
            var sentNumber = 1;
            var lineNumber = 1;
            for (; ; )
            {
                var r = ReadNextSent(textReader, ref lineNumber);
                if (!r)
                    break;

                if (0 < _words.Count)
                {
                    _posTaggerScriber.WriteCrfAttributesWords4ModelBuilder(textWriter, _words);
                }

                if ((sentNumber % sentNumberCallbackStep) == 0)
                {
                    processSentCallback(sentNumber);
                }
                sentNumber++;
            }
            if ((sentNumber % sentNumberCallbackStep) != 0)
            {
                processSentCallback(sentNumber);
            }

            return sentNumber;
        }

        unsafe private bool ReadNextSent(TextReader textReader, ref int lineNumber)
        {
            _words.Clear();

            for (var line = textReader.ReadLine(); ; line = textReader.ReadLine())
            {
                if (line == null)
                    return false;

                lineNumber++;

                if (string.IsNullOrWhiteSpace(line))
                    break;

                var a = line.Split(SPLIT_CHARS, StringSplitOptions.RemoveEmptyEntries);
                if (a.Length < 2)
                    throw new InvalidDataException($"Wrong input data format. APPROXIMITE-LINE-NUMBER: {lineNumber}, line-TEXT: '{line}{'\''}");

                var v = a[0].Trim().Replace('ё', 'е').Replace('Ё', 'Е');
                var p = ToPosTaggerOutputType(a[2].Trim());

                //skip url's
                var urls = _urlDetector.AllocateUrls(v);
                if (urls.Count != 0)
                {
                    continue;
                }

                fixed (char* ptr = v)
                {
                    var word = new Word()
                    {
                        valueOriginal = v,
                        valueUpper = v.ToUpperInvariant(),
                        posTaggerOutputType = p,
                    };
                    var result = _posTaggerInputTypeProcessor.GetResult(ptr, v.Length, word);
                    word.posTaggerInputType = result.posTaggerInputType;
                    word.posTaggerExtraWordType = result.posTaggerExtraWordType;

                    if ((word.posTaggerExtraWordType == PosTaggerExtraWordType.__DEFAULT__) && (word.posTaggerInputType == PosTaggerInputType.O))
                    {
                        if (word.valueOriginal.Contains(' '))
                        {
                            word.posTaggerInputType = PosTaggerInputType.CompPh;
                        }
                    }

                    _words.Add(word);
                }
            }

            return true;
        }

        private static PosTaggerOutputType ToPosTaggerOutputType(string value)
        {
            return (PosTaggerOutputType)Enum.Parse(typeof(PosTaggerOutputType), value, true);
        }

        private static IPosTaggerInputTypeProcessor CreatePosTaggerInputTypeProcessor(LanguageTypeEnum languageType)
        {
            switch (languageType)
            {
                case LanguageTypeEnum.En:
                    return new PosTaggerInputTypeProcessorEn(new HashSet<string>(), new HashSet<string>());

                case LanguageTypeEnum.Ru:
                    return new PosTaggerInputTypeProcessorRu(new HashSet<string>(), new HashSet<string>());

                default:
                    throw new ArgumentException(languageType.ToString());
            }
        }
    }
}
