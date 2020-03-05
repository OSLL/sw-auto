using AnalyzeResults.Presentation;
using System;
using System.Collections.Generic;
using System.IO;
using AnalyzeResults.Settings;
using TextExtractor;

namespace ConsoleTimeTestApp
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var filePaths = Directory.GetFiles(@"D:\programming\papersForTest");

            var docPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            using (StreamWriter outputFile = new StreamWriter(Path.Combine(docPath, $"result{DateTime.Now:MM_DD_hh_mm_ss}.txt")))
                foreach (var path in filePaths)
                {
                    Console.WriteLine($"{path} processing...");
                    var result = AnalyzePaper(path, string.Empty, string.Empty, string.Empty);
                    var file = new FileInfo(path);

                    var metrics = result.Metrics; 
                    outputFile.WriteLine($"{file.Name}\t{file.Length} bytes\t{metrics.StrLength} lines\t{metrics.TextExtractionTime} ms\t{metrics.AnalyzingTime} ms");
                }
        }

        public static PaperAnalysisResult AnalyzePaper(string path, string titles, string paperName, string refsName)
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            var textExtractor = new PdfTextExtractor(path);
            try
            {
                var text = textExtractor.GetAllText();
                watch.Stop();
                var textExtractionTime = watch.ElapsedMilliseconds;
                watch.Restart();
                var result = PaperAnalyzer.PaperAnalyzer.Instance.ProcessTextWithResult(text, titles, paperName, refsName, new ResultScoreSettings());
                watch.Stop();

                // Заполнение метрик процесса анализа
                var metrics = new PaperAnalysisProcessingMetrics()
                {
                    TextExtractionTime = textExtractionTime,
                    AnalyzingTime = watch.ElapsedMilliseconds,
                    StrLength = text.Length,
                };
                result.Metrics = metrics;

                return result;
            }
            catch (Exception ex)
            {
                var res = new PaperAnalysisResult(new List<Section>(), new List<Criterion>(), new List<AnalyzeResults.Errors.Error>())
                {
                    Error = ex.Message
                };
                return res;
            }
        }
    }
}
