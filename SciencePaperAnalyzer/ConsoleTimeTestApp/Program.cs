using AnalyzeResults.Presentation;
using System;
using System.Collections.Generic;
using System.IO;
using TextExtractor;

namespace ConsoleTimeTestApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var filePaths = Directory.GetFiles(@"D:\programming\papersForTest");

            string docPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            using (StreamWriter outputFile = new StreamWriter(Path.Combine(docPath, $"result{string.Format("{0:MM_DD_hh_mm_ss}", DateTime.Now)}.txt")))
                foreach (var path in filePaths)
                {
                    Console.WriteLine($"{path} processing...");
                    long textExtractionTime, analyzingTime;
                    int strLength;
                    AnalyzePaper(path, string.Empty, string.Empty, string.Empty, out textExtractionTime, out analyzingTime, out strLength);
                    var file = new FileInfo(path);
                    outputFile.WriteLine($"{file.Name}\t{file.Length} bytes\t{strLength} lines\t{textExtractionTime} ms\t{analyzingTime} ms");
                }
        }

        public static PaperAnalysisResult AnalyzePaper(string path, string titles, string paperName, string refsName,
            out long textExtractionTime, out long analyzingTime, out int stringLength)
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            var textExtractor = new PdfTextExtractor(path);
            try
            {
                var text = textExtractor.GetAllText();
                watch.Stop();
                textExtractionTime = watch.ElapsedMilliseconds;
                watch.Restart();
                var result = PaperAnalyzer.PaperAnalyzer.Instance.ProcessTextWithResult(text, titles, paperName, refsName);
                watch.Stop();
                analyzingTime = watch.ElapsedMilliseconds;
                stringLength = text.Length;
                return result;
            }
            catch (Exception ex)
            {
                var res = new PaperAnalysisResult(new List<Section>(), new List<Criterion>(), new List<AnalyzeResults.Errors.Error>());
                res.Error = ex.Message;
                textExtractionTime = -1;
                analyzingTime = -1;
                stringLength = -1;
                return res;
            }
        }
    }
}
