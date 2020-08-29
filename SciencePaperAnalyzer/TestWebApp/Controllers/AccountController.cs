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

        [HttpPost]
        public async Task<IActionResult> Register(RegisterModel model)
        {
            if (model.Login != null && model.Password == model.ConfirmPassword &&
                model.Password != null)
            {
                model.Role = "student";
                _users = await _context.GetUsers();
                User user = _users.FirstOrDefault(u => u.Login == model.Login);
                if (user == null)
                {
                    user = new User()
                    {
                        Login = model.Login, Password = model.Password,
                        Role = model.Role
                    };
                    await _context.AddUser(user);
                    await Authenticate(user);

                    return RedirectToAction("Index", "Home");
                }
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
                user = _users.FirstOrDefault(u => u.Login.Equals(model.Login) && u.Password.Equals(model.Password));
                if (user != null)
                {
                    await Authenticate(user);

                    if (user.Role == "teacher")
                        return RedirectToAction("TeacherMainPage", "StudentTeacher");
                    if (user.Role == "admin")
                        return RedirectToAction("AdminMainPage", "Admin");
                    return RedirectToAction("Index", "Home");
                }
            }
            return View(model);
        }
        private async Task Authenticate(User user)
        {
            
            var claims = new List<Claim>
            {
                new Claim(ClaimsIdentity.DefaultNameClaimType, user.Login),
                new Claim(ClaimsIdentity.DefaultRoleClaimType, user.Role)
            };
            
            ClaimsIdentity id = new ClaimsIdentity(claims, "ApplicationCookie", ClaimsIdentity.DefaultNameClaimType,
                ClaimsIdentity.DefaultRoleClaimType);
            
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(id));
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }
    }
}