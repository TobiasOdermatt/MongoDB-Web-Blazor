using BlazorServerMyMongo.Data.DB;
using BlazorServerMyMongo.Data.Helpers;
using BlazorServerMyMongo.Data.OTP;
using BlazorServerMyMongo.Objects;
using Microsoft.AspNetCore.Mvc;
using static BlazorServerMyMongo.Data.Helpers.LogManager;

namespace BlazorServerMyMongo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : Controller
    {
        private Object GenerateUUIDObject(string UUID)
        {
            string uuid = UUID;
            return new { uuid = uuid };
        }

        // If connection is successful, UUID will be returned
        [HttpPost("CreateOTP")]
        public IActionResult CreateOTP(ConnectRequestObject dataJSON)
        {
            if (Request.HttpContext.Connection.RemoteIpAddress == null) return NoContent();

            string ipOfRequest = Request.HttpContext.Connection.RemoteIpAddress.ToString();

            OTPManagement otpManagement = new();
            string? decryptedData = otpManagement.DecryptUserData(dataJSON.AuthCookieKey, dataJSON.RandData);

            if (decryptedData is null) return NoContent();

            (string username, string password) = otpManagement.getUserData(decryptedData);
            var uuid = Guid.NewGuid().ToString();

            DBConnector connector = new(username, password, uuid, Request.HttpContext.Connection.RemoteIpAddress.ToString());

            if (connector.Client != null)
            {
                var uuidObject = GenerateUUIDObject(uuid);

                DateTime localDate = DateTime.Now;
                OTPFileObject newFile = new(localDate, dataJSON.RandData);
                OTPFileManagement fileManger = new();

                fileManger.WriteOTPFile(uuid, newFile);
                fileManger.CleanUpOTPFiles();

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

            OTPFileManagement fileManger = new();
            fileManger.DeleteOTPFile(uuid);

            HttpContext.Response.Cookies.Delete("UUID");
            HttpContext.Response.Cookies.Delete("Token");

            return Redirect("/Connect");
        }
    }
}
