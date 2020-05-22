using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using AnalyzeResults.Settings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
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
        [Authorize(Roles = "teacher")]
        public IActionResult TeacherMainPage()
        {
            return View();
        }
        [HttpGet]
        [Authorize(Roles = "teacher")]
        public async Task<IActionResult> TeacherAddCriterion(bool mine)
        {
            _criteria = await _context.GetCriteria();
            ViewBag.Criteria = mine ? _criteria.Where(c => c.TeacherLogin == User.Identity.Name).ToList() : _criteria.ToList();
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "teacher")]
        public async Task<IActionResult> TeacherAddCriterion(ResultCriterion model)
        {
            _criteria = await _context.GetCriteria();
            if (_criteria.FirstOrDefault(u => u.Name == model.Name) == null)
            {
                model.TeacherLogin = User.Identity.Name;
                model.Recalculate();
                await _context.AddCriterion(model);
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
    }
}
