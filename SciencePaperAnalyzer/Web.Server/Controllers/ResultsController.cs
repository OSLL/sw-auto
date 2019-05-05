using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TextExtractor;
using Web.Shared;

namespace Web.Server.Controllers
{
    [Route("api/[controller]")]
    public class ResultsController : Controller
    {
        [HttpPost("[action]")]
        public async Task<IActionResult> AnalyzeLastPaper(TitlesInfo info)
        {
            await Task.Delay(1);
            var textExtractor = new PdfTextExtractor("result.pdf");
            try
            {
                var text = textExtractor.GetAllText();
                var result = PaperAnalyzer.PaperAnalyzer.Instance.ProcessTextWithResult(text, info.Titles, info.PaperName, info.RefsName);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}