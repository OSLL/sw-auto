using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AnalyzeResults.Presentation;
using AnalyzeResults.Settings;
using Microsoft.Extensions.Configuration;
using NLog;
using TextExtractor;
using WebPaperAnalyzer.Models;

namespace PaperAnalyzer.Service
{
    public interface IPaperAnalyzerService
    {
        ITextExtractor GetTextExtractor(string path);

        PaperAnalysisResult GetAnalyze(UploadFile file, string titles, string paperName, string refsName, string keywords,
            ResultScoreSettings settings);
    }

    public class PaperAnalyzerService : IPaperAnalyzerService
    {
        private readonly IPaperAnalyzer _paperAnalyzer;
        private readonly IConfiguration _appConfig;

        public PaperAnalyzerService(IPaperAnalyzer paperAnalyzer, IConfiguration appConfig = null)
        {
            _paperAnalyzer = paperAnalyzer;
            _appConfig = appConfig;
        }

        public PaperAnalysisResult GetAnalyze(UploadFile file, string titles, string paperName, string refsName, string keywords, ResultScoreSettings settings)
        {
            if (string.IsNullOrEmpty(file?.FileName))
            {
                throw new FileNotFoundException($"Filename is empty");
            }

            var ext = Path.GetExtension(file.FileName);

            paperName = (paperName == null ? "" : paperName);

            PaperAnalysisResult result;
            switch (ext) // partial migration, currently supported only md files
            {
                case ".md":
                    {
                        bool autoExtractPaperName = paperName.Equals(_appConfig == null ? "" : _appConfig.GetValue("AutoTitleExtractionToken", "#auto#"));
                        var extractor = GetTextExtractor(file.FileName);
                        List<Section> sections = extractor.ExtractStructureFromFileStream(file.DataStream, autoExtractPaperName);
                        result = _paperAnalyzer.ProcessTextWithResult(sections, paperName, keywords, settings);
                    }
                    break;
                default:
                    {
                        var extractor = GetTextExtractor(file.FileName);
                        var text = extractor.ExtractTextFromFileStream(file.DataStream);
                        result = _paperAnalyzer.ProcessTextWithResult(text, titles, paperName, refsName, keywords, settings);
                    }
                    break;
            }

            return result;
        }

        public ITextExtractor GetTextExtractor(string filename)
        {
            var ext = Path.GetExtension(filename);

            switch (ext)
            {
                case ".pdf":
                    return new PdfTextExtractor();

                case ".md":
                    return new MdTextExtractor();

                case ".docx":
                    return new DocxTextExtractor();

                default:
                    throw new NotImplementedException($"File type {ext} is not supported");

            }
        }
    }
}
