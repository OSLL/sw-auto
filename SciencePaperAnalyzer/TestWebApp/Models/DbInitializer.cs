using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;
using MongoDB.Driver;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AnalyzeResults.Settings;
using TestWebApp.Models;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Configuration;

namespace WebPaperAnalyzer.Models
{
    public class DbInitializer
    {
        IConfiguration _appConfig;
        private ApplicationContext _context;
        public DbInitializer(IConfiguration appConfig = null, IOptions<MongoSettings> mongoSettings = null)
        {
            var _mongoSettings = mongoSettings.Value;

            _context = new ApplicationContext(_mongoSettings);
            _appConfig = appConfig;
        }

        public async Task InitAdmin()
        {
            var adminInfo = _appConfig.GetSection("AdminAccount");

            var login = "admin";
            var password = Convert.ToBase64String(Guid.NewGuid().ToByteArray()).Substring(0, 8);
            if (adminInfo != null)
            {
                login = adminInfo.GetValue<string>("Login", "admin");
                password = adminInfo.GetValue<string>("Password", password);
            }
            var users = await _context.GetUsers();
            var admin = users.FirstOrDefault((user)=> (user.Login==login));
            if (admin==null)
            {
                await _context.AddUser(new User
                {
                    Login = login,
                    Password = password,
                    Role = "admin"
                });
                Console.WriteLine("Admin account created:");
                Console.WriteLine("Login: " + login);
                Console.WriteLine("Password: " + password);
            }else{
                Console.WriteLine("Admin account exists:");
                Console.WriteLine("Login: " + login);
                Console.WriteLine("Password: " + password);
            }
        }
    }
}