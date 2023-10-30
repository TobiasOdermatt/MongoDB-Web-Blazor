using MongoDB_Web.Data.DB;
using MongoDB_Web.Data.Helpers;
using MongoDB_Web.Data.OTP;
using MongoDB_Web.Objects;
using Microsoft.AspNetCore.Mvc;
using static MongoDB_Web.Data.Helpers.LogManager;

namespace MongoDB_Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : Controller
    {
        private readonly OTPFileManagement _otpFileManagement;

        public AuthController(OTPFileManagement otpFileManagement)
        {
            _otpFileManagement = otpFileManagement;
        }

        Object generateUUIDObject(string uuid)
        {
            return new { uuid };
        }

        // If connection is successful, UUID will be returned
        [HttpPost("CreateOTP")]
        public IActionResult CreateOTP(ConnectRequestObject dataJSON)
        {
            if (Request.HttpContext.Connection.RemoteIpAddress == null) 
                return NoContent();

            string ipOfRequest = Request.HttpContext.Connection.RemoteIpAddress.ToString();

            OTPManagement otpManagement = new();
            string? decryptedData = otpManagement.DecryptUserData(dataJSON.AuthCookieKey, dataJSON.RandData);

            if (decryptedData is null) 
                return NoContent();

            (string username, string password) = otpManagement.GetUserData(decryptedData);
            var uuid = Guid.NewGuid().ToString();

            DBConnector connector = new(username, password, uuid, Request.HttpContext.Connection.RemoteIpAddress.ToString());
            if(connector.ListAllDatabasesTest() == false)
                return NoContent(); 

            if (connector.Client != null)
            {
                var uuidObject = generateUUIDObject(uuid);

                DateTime localDate = DateTime.Now;
                OTPFileObject newFile = new(Guid.Parse(uuid), localDate, dataJSON.RandData, ipOfRequest, false, username);

                _otpFileManagement.WriteOTPFile(uuid, newFile);

                LogManager _ = new(LogType.Info, "OTP file created for user: " + username + " with UUID " + uuid + " IP: " + ipOfRequest);

                return new JsonResult(uuidObject);
            }

            return NoContent();
        }

        [HttpGet("Logout")]
        public IActionResult Logout()
        {
            string? uuid = HttpContext.Request.Cookies["UUID"];
            if (uuid is null)
                return Redirect("/Connect");

            _otpFileManagement.DeleteOTPFile(uuid);

            HttpContext.Response.Cookies.Delete("UUID");
            HttpContext.Response.Cookies.Delete("Token");

            return Redirect("/Connect");
        }
    }
}
