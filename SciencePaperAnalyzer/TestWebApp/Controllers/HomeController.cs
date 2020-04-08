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
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PaperAnalyzer;
using PaperAnalyzer.Service;
using TestWebApp.Models;
using WebPaperAnalyzer.DAL;
using WebPaperAnalyzer.Models;

namespace WebPaperAnalyzer.Controllers
{
    [System.Runtime.InteropServices.Guid("AC77F42B-4207-4468-A583-0999046DBAFD")]
    public class HomeController : Controller
    {

        private readonly ILogger<HomeController> _logger;

        protected IConfiguration Configuration;

        private readonly IPaperAnalyzerService _analyzeService;
        protected IResultRepository _repository;
        protected ResultScoreSettings ResultScoreSettings { get; set; }
        protected MongoSettings MongoSettings { get; set; }

        public HomeController(
            ILogger<HomeController> logger,
            IPaperAnalyzerService analyzeService,
            IResultRepository repository,
            IOptions<ResultScoreSettings> resultScoreSettings = null,
            IOptions<MongoSettings> mongoSettings = null,
            IConfiguration configuration = null)
        {
            ResultScoreSettings = resultScoreSettings?.Value;
            MongoSettings = mongoSettings?.Value;
            _repository = repository;
            Configuration = configuration;
            _logger = logger;
            _analyzeService = analyzeService;
        }

        [HttpPost]
        public async Task<IActionResult> UploadFile(IFormFile file, string titles, string paperName, string refsName)
        {
            if (file == null)
            {
                return new PartialViewResult();
            }

            var uploadFile = new UploadFile
            {
                FileName = file.FileName, 
                Length = file.Length, 
                DataStream = new MemoryStream()
            };

            await file.CopyToAsync(uploadFile.DataStream);

            var settings = new ResultScoreSettings
            {
                ErrorCost = double.Parse(Configuration.GetSection("ResultScoreSettings")["ErrorCost"]),
                KeyWordsCriterionFactor = double.Parse(Configuration.GetSection("ResultScoreSettings")["KeyWordsCriterionFactor"]),
                WaterCriterionFactor = double.Parse(Configuration.GetSection("ResultScoreSettings")["WaterCriterionFactor"]),
                ZipfFactor = double.Parse(Configuration.GetSection("ResultScoreSettings")["ZipfFactor"])
            };

            PaperAnalysisResult result;
            try
            {
                result = _analyzeService.GetAnalyze(uploadFile, titles, paperName, refsName, settings);
            }
            catch (Exception ex)
            {
                result = new PaperAnalysisResult(new List<Section>(), new List<Criterion>(),
                    new List<AnalyzeResults.Errors.Error>()) {Error = ex.Message};

                return Error(ex.Message);
            }
   
            var analysisResult = new AnalysisResult
            {
                Id = Guid.NewGuid().ToString(),
                Result = result
            };
            _repository.AddResult(analysisResult);

            return Ok(analysisResult.Id);
        }

        [HttpGet]
        public IActionResult Result(string id)
        {
            return View(_repository.GetResult(id).Result);
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
        public IActionResult Error(string message)
        {
            return View(new ErrorViewModel { 
                RequestId = Activity.Current?.Id ?? HttpContext?.TraceIdentifier,
                Message = message
            });
        }
    }
}
