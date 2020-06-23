using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AnalyzeResults.Settings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WebPaperAnalyzer.DAL;
using WebPaperAnalyzer.Models;
using WebPaperAnalyzer.ViewModels;

namespace TestWebApp.Controllers
{
    public class StudentTeacherController : Controller
    {
        private ApplicationContext _context;
        private IResultRepository _results;
        private IEnumerable<ResultCriterion> _criteria;
        private readonly ILogger<StudentTeacherController> _logger;
        public StudentTeacherController(IResultRepository repository, 
            ILogger<StudentTeacherController> logger,
            IOptions<MongoSettings> mongoSettings = null)
        {
            var _mongoSettings = mongoSettings.Value;
            _context = new ApplicationContext(_mongoSettings);
            _results = repository;
            _logger = logger;
        }

        [HttpGet]
        [Authorize(Roles = "teacher")]
        public IActionResult TeacherMainPage()
        {
            return View();
        }

        [HttpGet]
        [Authorize(Roles ="teacher")]
        public IActionResult AddDictionary()
        {
            _logger.LogDebug("Received Get request AddDictionary");
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "teacher")]
        public async Task<IActionResult> AddDictionary(DictionaryModel model)
        {
            _logger.LogDebug("Received Post request AddDictionary");
            Console.WriteLine("Received Post request AddDictionary");
            var dataStream = new MemoryStream();
            model.File.CopyTo(dataStream);
            _logger.LogDebug("dataStream successfull copied");
            _logger.LogDebug($"dataStream length: {dataStream.Length}");
            _logger.LogDebug($"dictionary name: {model.Name}");

            dataStream.Position = 0;
            var rows = new List<string>();
            using (var reader = new StreamReader(dataStream, Encoding.UTF8))
            {
                var line = reader.ReadLine();
                while (line != null)
                {
                    rows.Add(line);
                    _logger.LogDebug($"Add forbidden words: {line}");
                    line = reader.ReadLine();
                }
            }
            _logger.LogDebug($"Read from file {rows.Count} lines");

            var fw = new ForbiddenWords()
            {
                Name = model.Name,
                Words = rows,
            };
            await _context.AddDictionary(fw);
            _logger.LogDebug($"Successfull write to database");
            Console.WriteLine("Successfull write to database");
            return RedirectToAction("TeacherMainPage");
        }

        [HttpGet]
        [Authorize(Roles = "teacher")]
        public async Task<IActionResult> TeacherAddCriterion(bool mine)
        {
            _logger.LogDebug("Received Get request AddCriterion");
            Console.WriteLine("Received Get request AddCriterion");
            _criteria = await _context.GetCriteria();
            ViewBag.Criteria = mine ? _criteria.Where(c => c.TeacherLogin == User.Identity.Name).ToList() : _criteria.ToList();
            var dict = await _context.GetForbiddenWordDictionary();
            _logger.LogError($"Finded {dict.ToList().Count} dictionary");
            //ViewBag.Dicts = dict.Select(d => d.Name).ToList();
            _logger.LogDebug(string.Join(",", dict.Select(d => d.Name).ToList()));
            var model = new AddCriterion()
            {
                Dictionaries = dict.Select(x => new DictionaryCheckBoxModel() { Name = x.Name, IsSelected = false }).ToList(),
            };
            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "teacher")]
        public async Task<IActionResult> TeacherAddCriterion(AddCriterion model)
        {
            _logger.LogDebug("Received Post request AddCriterion");
            try
            {
                _logger.LogDebug($"Selected {model.Dictionaries.Count(x => x.IsSelected)} dictionaries");
                _logger.LogDebug(
                    $"Name selected dictionary: {string.Join(",", model.Dictionaries.Where(x => x.IsSelected).Select(x => x.Name))}");
            }
            catch (Exception)
            {
                _logger.LogDebug("No selected dictionaries");
            }

            _criteria = await _context.GetCriteria();
            ResultCriterion criterion = _criteria.FirstOrDefault(u => u.Name == model.Name);
            if (criterion == null)
            {
                criterion = model;
                criterion.TeacherLogin = User.Identity.Name;
                try
                {
                    criterion.ForbiddenWordDictionary =
                        model.Dictionaries.Where(x => x.IsSelected).Select(x => x.Name);
                }
                catch (Exception)
                {
                    criterion.ForbiddenWordDictionary = null;
                }

                criterion.Recalculate();
                await _context.AddCriterion(criterion);
            }
            return RedirectToAction("TeacherAddCriterion", "StudentTeacher", new {mine = false});
        }
        
        [HttpGet]
        [Authorize(Roles = "teacher")]
        public IActionResult TeacherViewResults()
        {
            return View(_results.GetResultsByLogin(User.Identity.Name, true).Where(res => res.StudentLogin != null));
        }

        [HttpGet]
        [Authorize(Roles = "teacher")]
        public IActionResult EditDeleteCriterion(string name)
        {
            var criterion = _context.GetCriteriaByName(name);
            return View(criterion);
        }

        [HttpPost]
        [Authorize(Roles = "teacher")]
        public async Task<IActionResult> EditCriterion(ResultCriterion editCriterion)
        {
            editCriterion.Recalculate();
            await _context.EditCriterion(editCriterion);
            return RedirectToAction("EditDeleteCriterion", "StudentTeacher",new {name = editCriterion.Name});
        }

        [HttpGet]
        [Authorize(Roles = "teacher")]
        public async Task<IActionResult> DeleteCriterion(string id)
        {
            await _context.DeleteCriterion(id);
            return RedirectToAction("TeacherAddCriterion", "StudentTeacher", new {mine = false});
        }

        [HttpGet]
        [Authorize(Roles = "teacher")]
        public async Task<FileResult> DownloadDictionary(string name)
		{
            var dictionary = await _context.GetDictionary(name);
            var fileBytes = ConvertToByteArray(dictionary);
            var fileName = $"{dictionary.Name}.txt";

            return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
		}

        [HttpPost]
        [Authorize(Roles = "teacher")]
        public async Task<IActionResult> DeleteDictionary(string name)
		{
            await _context.DeleteDictionary(name);
            return RedirectToAction("TeacherMainPage");
        }

        private byte[] ConvertToByteArray(ForbiddenWords dictionary)
		{
            var stringBuilder = new StringBuilder();
            foreach (var word in dictionary.Words)
			{
                stringBuilder.Append(word);
                stringBuilder.AppendLine();
			}
            var encoder = new UTF8Encoding();
            var result = encoder.GetBytes(stringBuilder.ToString());
            return result;
		}
    }
}
