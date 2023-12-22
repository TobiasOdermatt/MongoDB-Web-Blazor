using Microsoft.AspNetCore.Mvc;
using MongoDB_Web.Data.OTP;
using System.Net.Mime;

namespace MongoDB_Web.Controllers
{
    [ApiController]
    public class FileController : ControllerBase
    {
        string userStoragePath = $"{Directory.GetCurrentDirectory()}" + @"\UserStorage\";
        private readonly AuthManager _authManager;

        public FileController(AuthManager otpAuthCookieManagement, OTPFileManagement otpFileManagement)
        {
            _authManager = otpAuthCookieManagement;
        }

        [HttpGet("DownloadFile")]
        public IActionResult DownloadFile(string fileName)
        {
            if (!_authManager.IsCookieValid())
                return NotFound();

            string? userUUID = _authManager.GetUUID();
        
            string fullPath = Path.Combine(userStoragePath, userUUID+"/"+"downloads"+"/"+fileName);

            if (!System.IO.File.Exists(fullPath))
                return NotFound("File not found");


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

        [HttpPost("UploadFile")]
        public IActionResult UploadFile(IFormFile file)
        {
            int chunkIndex;
            int totalChunks;

            if (!int.TryParse(HttpContext.Request.Form["chunkIndex"], out chunkIndex))
                chunkIndex = 0;

            if (!int.TryParse(HttpContext.Request.Form["totalChunks"], out totalChunks))
                totalChunks = 0;

            string guid = HttpContext.Request.Form["guid"].ToString();

            if (!_authManager.IsCookieValid())
                return NotFound();

            string? userUUID = _authManager.GetUUID();

            if (file == null || file.Length == 0)
                return BadRequest("Invalid file");

            string userUploadPath = Path.Combine(userStoragePath, userUUID + "/" + "uploads" + "/");

            if (!Directory.Exists(userUploadPath))
                Directory.CreateDirectory(userUploadPath);

            string fullPath = userUploadPath + file.FileName;

            if (!System.IO.File.Exists(fullPath))
                System.IO.File.Create(fullPath).Close();


            FileMode fileMode = (chunkIndex == 0) ? FileMode.Create : FileMode.Append;

            try
            {
                using (FileStream stream = new(fullPath, fileMode))
                    file.CopyTo(stream);


                if (chunkIndex == totalChunks - 1)
                {
                    return Ok();
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex}");
            }
        }
    }
}