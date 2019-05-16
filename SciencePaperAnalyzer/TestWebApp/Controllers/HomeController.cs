using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AnalyzeResults.Presentation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TestWebApp.Models;
using TextExtractor;

namespace TestWebApp.Controllers
{
    [System.Runtime.InteropServices.Guid("AC77F42B-4207-4468-A583-0999046DBAFD")]
    public class HomeController : Controller
    {
        public static List<PaperAnalysisResult> Results = new List<PaperAnalysisResult>();
        public static PaperAnalyzer.PaperAnalyzer Analyzer = PaperAnalyzer.PaperAnalyzer.Instance;

        [HttpPost]
        public async Task<IActionResult> UploadFile(IFormFile file, string titles, string paperName, string refsName)
        {
            if (file == null)
            {
                return new PartialViewResult();
            }
            // full path to file in temp location
            var filePath = Path.GetTempFileName();

            if (file.Length > 0)
            {
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }
            }

            var result = AnalyzePaper(filePath, titles, paperName, refsName);
            if (Results.Count > 0)
            {
                Results.Clear();
            }
            Results.Add(result);
            return Ok();
        }

        [HttpGet]
        public IActionResult Result()
        {
            return View(Results.Count > 0 ? Results.Last() : null);
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public static PaperAnalysisResult AnalyzePaper(string path, string titles, string paperName, string refsName)
        {
            var textExtractor = new PdfTextExtractor(path);
            try
            {
                var text = textExtractor.GetAllText();

                return Analyzer.ProcessTextWithResult(text, titles, paperName, refsName);
            }
            catch (Exception ex)
            {
                var res = new PaperAnalysisResult(new List<Section>(), new List<Criterion>(), new List<AnalyzeResults.Errors.Error>());
                res.Error = ex.Message;
                return res;
            }
        }
    }
}
