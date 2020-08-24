using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AnalyzeResults.Settings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;
using WebPaperAnalyzer.Models;
using WebPaperAnalyzer.ViewModels;

namespace WebPaperAnalyzer.Controllers
{
    public class AdminController : Controller
    {
        private ApplicationContext _context;

        public AdminController(IOptions<MongoSettings> mongoSettings = null)
        {
            _context = new ApplicationContext(mongoSettings?.Value);
        }

        [HttpGet]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> AddTeacher()
        {
            var users = await _context.GetUsers();
            var teachers = users.Where(u => u.Role.Equals("teacher"));

            ViewBag.Teachers = teachers;

            return View();
        }

        [HttpPost]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> AddTeacher(LoginModel model)
        {
            if (model.Login != null && model.Password != null)
            {
                var users = await _context.GetUsers();
                var user = users.FirstOrDefault(u => u.Login == model.Login);
                if (user == null)
                {
                    user = new User()
                    {
                        Login = model.Login,
                        Password = model.Password,
                        Role = "teacher"
                    };

                    await _context.AddUser(user);
                }
            }

            return RedirectToAction("AddTeacher", "Admin");
        }
    }
}
