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
using Microsoft.Extensions.Options;
using TestWebApp.Models;
using TextExtractor;
using WebPaperAnalyzer.Models;
using WebPaperAnalyzer.DAL;
using Microsoft.Extensions.Logging;

namespace TestWebApp.Controllers
{
    [System.Runtime.InteropServices.Guid("AC77F42B-4207-4468-A583-0999046DBAFD")]
    public class HomeController : Controller
    {
        public static PaperAnalyzer.PaperAnalyzer Analyzer = PaperAnalyzer.PaperAnalyzer.Instance;
        IResultRepository repository;
        private readonly ILogger<HomeController> _logger;
        private ApplicationContext _context;
        private List<ResultCriterion> _criteria;

        protected IConfiguration Configuration;
        protected ResultScoreSettings ResultScoreSettings { get; set; }
        protected MongoSettings MongoSettings { get; set; }

        public HomeController(ApplicationContext context,
            ILogger<HomeController> logger,
            IOptions<ResultScoreSettings> resultScoreSettings = null,
            IOptions<MongoSettings> mongoSettings = null,
            IConfiguration configuration = null)
        {
            if (resultScoreSettings != null)
                ResultScoreSettings = resultScoreSettings.Value;
            MongoSettings = mongoSettings.Value;
            repository = new ResultRepository(MongoSettings);
            Configuration = configuration;
            _logger = logger;
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> UploadFile(IFormFile file, string titles, string paperName, string refsName,
                                                    string criterionName)
        {
            if (file == null)
            {
                return new PartialViewResult();
            }
            // full path to file in temp location
            var filePath = Path.GetTempFileName();
            _logger.LogDebug($"UploadFile: new file path: {filePath}");
            if (file.Length > 0)
            {
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }
            }
            _logger.LogDebug($"UploadFile: file saved");

            var criterion = _criteria.First(c => c.Name == criterionName);

            var settings = new ResultScoreSettings
                {
                    ErrorCost = criterion.ErrorCost,
                    KeyWordsCriterionFactor = criterion.KeyWordsCriterionFactor,
                    WaterCriterionFactor = criterion.WaterCriterionFactor,
                    ZipfFactor = criterion.ZipfFactor
                };
            var result = AnalyzePaper(filePath, titles, paperName, refsName, settings);
            _logger.LogDebug($"UploadFile: file analyzed");
            var analysisResult = new AnalysisResult
            {
                Id = Guid.NewGuid().ToString(),
                Result = result
            };
            repository.AddResult(analysisResult);
            _logger.LogDebug($"UploadFile: result saved");
            return Ok(analysisResult.Id);
        }

        [HttpGet]
        public IActionResult Result(string id)
        {
            return View(repository.GetResult(id).Result);
        }

        [Authorize]
        public IActionResult Index()
        {
            string tLogin;
            try
            {
                tLogin = _context.Users.Where(u => u.Login == User.Identity.Name).Select(u => u.TeacherLogin).First();
            }
            catch (Exception)
            {
                tLogin = null;
            }
            
            if (User.Identity.Name == tLogin)
                return RedirectToAction("TeacherAddCriterion", "StudentTeacher");
            _criteria = _context.Criteria.Where(c => c.TeacherLogin == tLogin || c.Id == 1).ToList();
            SelectList criteria = new SelectList(_criteria.Select(c => c.Name));
            ViewBag.Criteria = criteria;
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

        public PaperAnalysisResult AnalyzePaper(string path, string titles, string paperName, string refsName, ResultScoreSettings settings)
        {
            var textExtractor = new PdfTextExtractor(path);
            try
            {
                var text = textExtractor.GetAllText();
                _logger.LogTrace($"AnalyzePaper: text extracted");

                return Analyzer.ProcessTextWithResult(text, titles, paperName, refsName, settings);
            }
            catch (Exception ex)
            {
                _logger.LogTrace($"AnalyzePaper: ERROR - {ex.Message}:\n {(ex.InnerException != null ? ex.InnerException.Message : "")}");
                var res = new PaperAnalysisResult(new List<Section>(), new List<Criterion>(), new List<AnalyzeResults.Errors.Error>());
                res.Error = ex.Message;
                return res;
            }
        }
    }
}
