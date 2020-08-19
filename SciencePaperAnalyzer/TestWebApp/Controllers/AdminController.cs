using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AnalyzeResults.Settings;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;
using WebPaperAnalyzer.Models;

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
        public async Task<IActionResult> AddTeacher()
        {
            try
            {
                var users = await _context.GetUsers();
                var teachers = users.Where(u => u.Role == "teacher");

                ViewBag.Teachers = teachers;

                return View();
            }
            catch (Exception e)
            {
                return Content(e.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddTeacher(User user)
        {
            if (user.Login != null && user.Password != null)
            {
                var _users = await _context.GetUsers();
                if (_users.FirstOrDefault(u => u.Login == user.Login) == null)
                {
                    user.Role = "teacher";

                    await _context.AddUser(user);
                }
            }
            return View();
        }
    }
}
