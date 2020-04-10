using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AnalyzeResults.Presentation;
using AnalyzeResults.Settings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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

            ResultScoreSettings settings = null;

            if (criterion != null)
            {
                settings = new ResultScoreSettings
                {
                    ErrorCost = criterion.ErrorCost,
                    KeyWordsCriterionFactor = criterion.KeyWordsCriterionFactor,
                    WaterCriterionFactor = criterion.WaterCriterionFactor,
                    ZipfFactor = criterion.ZipfFactor
                };
            }
            else
            {
                //Возможно только во время выполнения теста
                settings = new ResultScoreSettings()
                {
                    ErrorCost = 2,
                    KeyWordsCriterionFactor = 35,
                    WaterCriterionFactor = 35,
                    ZipfFactor = 30
                };
            }

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

            Repository.AddResult(analysisResult);
            return Ok(analysisResult.Id);
        }

        [HttpGet]
        public IActionResult Result(string id)
        {
            return View(Repository.GetResult(id).Result);
        }

        public async Task<IActionResult> Index()
        {
            if (User.Identity.Name != null && User.FindFirst(x => x.Type == ClaimsIdentity.DefaultRoleClaimType).Value == "teacher")
                return RedirectToAction("TeacherMainPage", "StudentTeacher");
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
    }
}
