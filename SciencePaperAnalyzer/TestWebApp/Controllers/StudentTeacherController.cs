using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using AnalyzeResults.Settings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using WebPaperAnalyzer.Models;
using WebPaperAnalyzer.ViewModels;

namespace TestWebApp.Controllers
{
    public class StudentTeacherController : Controller
    {
        private ApplicationContext _context;
        private IEnumerable<ResultCriterion> _criteria;
        public StudentTeacherController(IOptions<MongoSettings> mongoSettings = null)
        {
            var _mongoSettings = mongoSettings.Value;
            _context = new ApplicationContext(_mongoSettings);
        }
        [HttpGet]
        public async Task<IActionResult> TeacherAddCriterion()
        {
            _criteria = await _context.GetCriteria();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> TeacherAddCriterion(AddCriterion model)
        {
            if (ModelState.IsValid)
            {
                ResultCriterion criterion = null;
                try
                {
                    criterion = _criteria.First(u => u.Name == model.Name);
                }
                catch (Exception)
                {
                    // ignored
                }

                if (criterion == null)
                {
                    criterion = new ResultCriterion()
                    {
                        Name = model.Name, TeacherLogin = User.Identity.Name,
                        ErrorCost = double.Parse(model.ErrorCost),
                        ZipfFactor = double.Parse(model.ZipfFactor),
                        WaterCriterionFactor = double.Parse(model.WaterCriterionFactor),
                        KeyWordsCriterionFactor = double.Parse(model.KeyWordsCriterionFactor)
                    };
                    await _context.AddCriterion(criterion);
                }

            }
            return View();
        }
    }
}
