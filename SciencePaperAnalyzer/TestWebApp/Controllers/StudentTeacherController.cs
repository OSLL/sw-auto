using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebPaperAnalyzer.Models;
using WebPaperAnalyzer.ViewModels;

namespace TestWebApp.Controllers
{
    public class StudentTeacherController : Controller
    {
        private ApplicationContext _context;
        public StudentTeacherController(ApplicationContext context)
        {
            _context = context;
        }
        [HttpGet]
        public IActionResult TeacherAddCriterion()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> TeacherAddCriterion(AddCriterion model)
        {
            if (ModelState.IsValid)
            {
                ResultCriterion criterion = await _context.Criteria.FirstOrDefaultAsync(u => u.Name == model.Name);
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
                    _context.Criteria.Add(criterion);
                    await _context.SaveChangesAsync();
                }

            }
            return View();
        }
    }
}
