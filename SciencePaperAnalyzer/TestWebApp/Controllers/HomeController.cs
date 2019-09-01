using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using AnalyzeResults.Presentation;
using AnalyzeResults.Settings;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using TestWebApp.Models;
using TextExtractor;
using WebPaperAnalyzer.Models;
using WebPaperAnalyzer.DAL;

namespace TestWebApp.Controllers
{
    [System.Runtime.InteropServices.Guid("AC77F42B-4207-4468-A583-0999046DBAFD")]
    public class HomeController : Controller
    {
        public static PaperAnalyzer.PaperAnalyzer Analyzer = PaperAnalyzer.PaperAnalyzer.Instance;
        IResultRepository repository = new ResultRepository("mongodb://localhost:27017/resultsDB");

        protected IConfiguration Configuration;
        protected ResultScoreSettings ResultScoreSettings { get; set; }

        public HomeController(IOptions<ResultScoreSettings> settings = null, IConfiguration configuration = null)
        {
            if (settings != null)
                ResultScoreSettings = settings.Value;
            Configuration = configuration;
        }

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
            var analysisResult = new AnalysisResult
            {
                Id = Guid.NewGuid().ToString(),
                Result = result
            };
            repository.AddResult(analysisResult);
            return Ok(analysisResult.Id);
        }

        [HttpGet]
        public IActionResult Result(string id)
        {
            return View(repository.GetResult(id).Result);
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

        public PaperAnalysisResult AnalyzePaper(string path, string titles, string paperName, string refsName)
        {
            var textExtractor = new PdfTextExtractor(path);
            try
            {
                var text = textExtractor.GetAllText();
                // get last from config (updated in runtime)
                var settings = new ResultScoreSettings
                {
                    ErrorCost = double.Parse(Configuration.GetSection("ResultScoreSettings")["ErrorCost"]),
                    KeyWordsCriterionFactor = double.Parse(Configuration.GetSection("ResultScoreSettings")["KeyWordsCriterionFactor"]),
                    WaterCriterionFactor = double.Parse(Configuration.GetSection("ResultScoreSettings")["WaterCriterionFactor"]),
                    ZipfFactor = double.Parse(Configuration.GetSection("ResultScoreSettings")["ZipfFactor"])
                };
                return Analyzer.ProcessTextWithResult(text, titles, paperName, refsName, settings);
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
