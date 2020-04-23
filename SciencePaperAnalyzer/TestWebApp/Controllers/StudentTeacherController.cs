using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using AnalyzeResults.Settings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
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
        public StudentTeacherController(IResultRepository repository, IOptions<MongoSettings> mongoSettings = null)
        {
            var _mongoSettings = mongoSettings.Value;
            _context = new ApplicationContext(_mongoSettings);
            _results = repository;
        }

        [HttpGet]
        public IActionResult TeacherMainPage()
        {
            return View();
        }
        [HttpGet]
        public async Task<IActionResult> TeacherAddCriterion()
        {
            _criteria = await _context.GetCriteria();
            _criteria = await _context.GetCriteria();
            ViewBag.Criteria = _criteria.ToList();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> TeacherAddCriterion(AddCriterion model)
        {
            if (ModelState.IsValid)
            {
                _criteria = await _context.GetCriteria();
                ResultCriterion criterion = _criteria.FirstOrDefault(u => u.Name == model.Name);

                if (criterion == null)
                {
                    criterion = new ResultCriterion()
                    {
                        Name = model.Name, TeacherLogin = User.Identity.Name,
                        ErrorCost = double.Parse(model.ErrorCost),
                        ZipfFactor = double.Parse(model.ZipfFactor),
                        ZipfFactorLowerBound = double.Parse(model.ZipfFactorLowerBound),
                        ZipfFactorUpperBound = double.Parse(model.ZipfFactorUpperBound),
                        WaterCriterionFactor = double.Parse(model.WaterCriterionFactor),
                        WaterCriterionLowerBound = double.Parse(model.WaterCriterionLowerBound),
                        WaterCriterionUpperBound = double.Parse(model.WaterCriterionUpperBound),
                        KeyWordsCriterionFactor = double.Parse(model.KeyWordsCriterionFactor),
                        KeyWordsCriterionLowerBound = double.Parse(model.KeyWordsCriterionLowerBound),
                        KeyWordsCriterionUpperBound = double.Parse(model.KeyWordsCriterionUpperBound)
                    };
                    await _context.AddCriterion(criterion);
                }
            }

            return RedirectToAction("TeacherAddCriterion", "StudentTeacher");
        }
        
        [HttpGet]
        public IActionResult TeacherViewResults()
        {
            return View(_results.GetResultsByLogin(User.Identity.Name, true).Where(res => res.StudentLogin != null));
        }
    }
}
