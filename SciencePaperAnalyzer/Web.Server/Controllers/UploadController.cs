using System.IO;
using System.Threading.Tasks;
using Web.Shared;
using Microsoft.AspNetCore.Mvc;

namespace Web.Server.Controllers
{
    [Route("api/[controller]")]
    public class UploadController : Controller
    {
        [HttpPost("[action]")]
        public async Task<IActionResult> Save()
        {
            var tempFileName = "result.pdf"; // Path.GetTempFileName()
            if (System.IO.File.Exists(tempFileName))
                System.IO.File.Delete(tempFileName);
            using (var writer = System.IO.File.OpenWrite(tempFileName))
            {
                await Request.Body.CopyToAsync(writer);
            }
            return Ok(new FileUploadResult { TempFileName = Path.GetFileNameWithoutExtension(tempFileName) });
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> Test()
        {
            await Task.Delay(1);
            return Ok("123");
        }
    }
}