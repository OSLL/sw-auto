using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AnalyzeResults.Presentation;
using AnalyzeResults.Settings;
using DocumentFormat.OpenXml.EMMA;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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
        private ApplicationContext _context;

        private readonly IPaperAnalyzerService _analyzeService;
        protected IResultRepository Repository;
        protected MongoSettings MongoSettings { get; set; }

        public HomeController(
            ILogger<HomeController> logger,
            IPaperAnalyzerService analyzeService,
            IResultRepository repository,
            IOptions<MongoSettings> mongoSettings = null)
        {
            MongoSettings = mongoSettings?.Value;
            Repository = repository;
            _context = new ApplicationContext(MongoSettings);
            _logger = logger;
            _analyzeService = analyzeService;
        }

        [HttpPost]
        public async Task<IActionResult> UploadFile(IFormFile file, string titles, string paperName, string refsName,
                                                    string criterionName = null)
        {
            _logger.LogInformation($"Received request UploadFile with criterionName {criterionName}");
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

            ResultCriterion criterion = null;

            try
            {
                var criteria = await _context.GetCriteria();
                criterion = criteria.First(c => c.Name == criterionName);
            }
            catch (Exception)
            {
                //Возможно только во время выполнения теста
            }

            ResultScoreSettings settings;

            if (criterion != null)
            {

                _logger.LogInformation($"Upload forbiddenwords dictionary: {string.Join(",", criterion.ForbiddenWordDictionary)}");
                settings = new ResultScoreSettings
                {
                    ErrorCost = criterion.ErrorCost,
                    KeyWordsCriterionFactor = criterion.KeyWordsCriterionFactor,
                    KeyWordsCriterionLowerBound = criterion.KeyWordsCriterionLowerBound,
                    KeyWordsCriterionUpperBound = criterion.KeyWordsCriterionUpperBound,
                    WaterCriterionFactor = criterion.WaterCriterionFactor,
                    WaterCriterionLowerBound = criterion.WaterCriterionLowerBound,
                    WaterCriterionUpperBound = criterion.WaterCriterionUpperBound,
                    ZipfFactor = criterion.ZipfFactor,
                    ZipfFactorLowerBound = criterion.ZipfFactorLowerBound,
                    ZipfFactorUpperBound = criterion.ZipfFactorUpperBound,
                    ForbiddenWords = await GetForbiddenWords(criterion.ForbiddenWordDictionary),
                };
            }
            else
            {
                //Возможно только во время выполнения теста
                settings = new ResultScoreSettings()
                {
                    ErrorCost = 2,
                    KeyWordsCriterionFactor = 35,
                    KeyWordsCriterionUpperBound = 6,
                    KeyWordsCriterionLowerBound = 14,
                    WaterCriterionFactor = 35,
                    WaterCriterionLowerBound = 14,
                    WaterCriterionUpperBound = 20,
                    ZipfFactor = 30,
                    ZipfFactorLowerBound = 5.5,
                    ZipfFactorUpperBound = 9.5,
                    ForbiddenWords = new List<ForbiddenWords>(),
                };
            }

            PaperAnalysisResult result;
            try
            {
                _logger.LogInformation($"Settings have {settings.ForbiddenWords.Count(x => true)} dictionary");
                result = _analyzeService.GetAnalyze(uploadFile, titles, paperName, refsName, settings);
            }
            catch (Exception ex)
            {
                return Error(ex.Message);
            }

            AnalysisResult analysisResult = null;
            if (criterion != null)
            {
                analysisResult = new AnalysisResult
                {
                    Id = Guid.NewGuid().ToString(),
                    Result = result,
                    StudentLogin = User.Identity.Name,
                    TeacherLogin = criterion.TeacherLogin,
                    Criterion = criterion.Name
                };
            }
            else
            {
                analysisResult = new AnalysisResult
                {
                    Id = Guid.NewGuid().ToString(),
                    Result = result,
                    StudentLogin = null,
                    TeacherLogin = null,
                    Criterion = null
                };
            }

            _logger.LogDebug($"Result saved by Id {analysisResult.Id}");
            Repository.AddResult(analysisResult);
            return Ok(analysisResult.Id);
        }

        [HttpGet]
        public IActionResult Result(string id)
        {
            _logger.LogDebug($"Try to show result by ID: {id}");
            return View(Repository.GetResult(id).Result);
        }

        public async Task<IActionResult> Index()
        {
            var criteria = await _context.GetCriteria();
            SelectList criteriaList = new SelectList(criteria.ToList().Select(c => c.Name));
            ViewBag.Criteria = criteriaList;
            return View();
        }

        [HttpGet]
        public IActionResult PreviousResults()
        {
            return View(Repository.GetResultsByLogin(User.Identity.Name, false));
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

        private async Task<IEnumerable<ForbiddenWords>> GetForbiddenWords(IEnumerable<string> forbiddenDictNames)
        {
            var res = new List<ForbiddenWords>();
            foreach (var dict in forbiddenDictNames)
            {
                _logger.LogInformation($"Try to upload dictionary with name: {dict}");
                var item = await _context.GetDictionary(dict);
                res.Add(item);
                _logger.LogInformation($"Added forbidden words: {string.Join(",", item.Words)}");
            }
            return res;
        }
    }
}
