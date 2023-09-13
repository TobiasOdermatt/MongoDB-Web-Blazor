using Microsoft.AspNetCore.Mvc;
using Microsoft.JSInterop;
using MongoDB_Web.Data.OTP;
using System.Net.Mime;

namespace MongoDB_Web.Controllers
{
    [ApiController]
    public class FileController : ControllerBase
    {
        string userFilePath = $"{Directory.GetCurrentDirectory()}" + @"\Output\";

        [HttpGet("DownloadFile")]
        public IActionResult DownloadFile(string fileName)
        {

            OTPAuthCookieManagement AuthManager = new(HttpContext);
            if (!AuthManager.IsCookieValid())
                return NotFound();

            string? userUUID = AuthManager.GetUUID();
        
            string fullPath = Path.Combine(userFilePath, userUUID+"/"+fileName);

            if (!System.IO.File.Exists(fullPath))
            {
                return NotFound("File not found");
            }

            var stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read);

            try
            {
                return File(stream, MediaTypeNames.Application.Octet, Path.GetFileName(fullPath));
            }
            catch (Exception ex)
            {
                stream.Close();
                return StatusCode(500, $"Internal server error: {ex}");
            }
        }
    }
}