using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TestWebApp.Models;
using TextExtractor;

namespace TestWebApp.Controllers
{
    public class HomeController : Controller
    {
        [HttpPost]
        public async Task<IActionResult> UploadFile(IFormFile file, string titles, string paperName, string refsName)
        {
            if (file == null)
            {
                return Ok("Select file!");
            }
            // full path to file in temp location
            var filePath = Path.GetTempFileName();

            if (file.Length > 0)
            {
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }
            }

            var result = TestITextSharp(filePath, titles, paperName, refsName);

            return Ok(result);
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public static string TestITextSharp(string path, string titles, string paperName, string refsName)
        {
            var textExtractor = new PdfTextExtractor(path);
            try
            {
                var text = textExtractor.GetAllText();

                return PaperAnalyzer.PaperAnalyzer.Instance.ProcessText(text, titles, paperName, refsName);
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
    }
}
