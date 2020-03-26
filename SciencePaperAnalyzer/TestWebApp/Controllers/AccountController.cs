using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AnalyzeResults.Settings;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity.UI.V3.Pages.Internal.Account;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;
using WebPaperAnalyzer.Models;
using WebPaperAnalyzer.ViewModels;

namespace TestWebApp.Controllers
{
    public class AccountController : Controller
    {
        private ApplicationContext _context;
        private IEnumerable<User> _users;
        public AccountController(IOptions<MongoSettings> mongoSettings = null)
        {
            var _mongoSettings = mongoSettings.Value;
            _context = new ApplicationContext(_mongoSettings);
        }
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }
        [HttpGet]
        public async Task<IActionResult> RegisterStudent()
        {
            _users = await _context.GetUsers();
            SelectList teachers = new SelectList(_users.Where(u => u.Role.Equals("teacher")).Select(u => u.Login));
            ViewBag.Teachers = teachers;
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterStudent(RegisterModel model)
        {
            _users = await _context.GetUsers();
            if (model.Password != null && model.Login != null && model.TeacherLogin != null
                && model.Password == model.ConfirmPassword)
            {
                User user = null;
                try
                {
                    user = _users.First(u => u.Login == model.Login);
                }
                catch (Exception e)
                {
                    //It's okay
                }
                if (user == null)
                {
                    user = new User
                    {
                        Login = model.Login, Password = model.Password,
                        TeacherLogin = model.TeacherLogin, Role = "student"
                    };

                    await _context.AddUser(user);

                    await Authenticate(user);

                    return RedirectToAction("Index", "Home");
                }
            }
            return View(model);
        }
        [HttpGet]
        public IActionResult RegisterTeacher()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterTeacher(RegisterModel model)
        {
            _users = await _context.GetUsers();

            if (model.Password != null && model.Login != null && model.Password == model.ConfirmPassword)
            {
                User user = null;
                try
                {
                    user = _users.First(u => u.Login == model.Login);
                }
                catch (Exception e)
                {
                    //It's okay
                }

                if (user == null)
                {
                    user = new User
                    {
                        Login = model.Login, Password = model.Password,
                        TeacherLogin = model.Login, Role = "teacher"
                    };

                    await _context.AddUser(user);

                    await Authenticate(user);

                    return RedirectToAction("Index", "Home");
                }

                ModelState.AddModelError("", "Некорректные логин и(или) пароль");
            }
            return View(model);
        }
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginModel model)
        {
            _users = await _context.GetUsers();
            if (model.Login != null && model.Password != null)
            {
                User user = null;
                user = _users.First(u => u.Login.Equals(model.Login) && u.Password.Equals(model.Password));
                if (user != null)
                {
                    await Authenticate(user);

                    return RedirectToAction("Index", "Home");
                }
            }
            return View(model);
        }
        private async Task Authenticate(User user)
        {
            
            var claims = new List<Claim>
            {
                new Claim(ClaimsIdentity.DefaultNameClaimType, user.Login)
            };
            
            ClaimsIdentity id = new ClaimsIdentity(claims, "ApplicationCookie", ClaimsIdentity.DefaultNameClaimType,
                ClaimsIdentity.DefaultRoleClaimType);
            
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(id));
        }
    }
}