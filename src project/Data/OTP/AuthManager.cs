using MongoDB_Web.Data.DB;
using MongoDB_Web.Data.Helpers;
using MongoDB_Web.Objects;
using System.Net;

namespace MongoDB_Web.Data.OTP
{
    public class AuthManager
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly OTPFileManagement _otpFileManagement;
        private readonly ConfigManager _configManager;

        public AuthManager(IHttpContextAccessor httpContextAccessor, OTPFileManagement otpFileManagement, ConfigManager configManager)
        {
            _httpContextAccessor = httpContextAccessor;
            _otpFileManagement = otpFileManagement;
            _configManager = configManager;
        }

        //Checks if the AuthCookie is valid
        public bool IsCookieValid()
        {
            (string? uuid, string? authOTP) = ReadOTPCookie();
            if (uuid is null || authOTP is null)
                return false;

            OTPFileObject? otpFile = _otpFileManagement.ReadOTPFile(uuid);

            if (otpFile is null)
                return false;

            OTPManagement otpManager = new();
            string? decryptedData = otpManager.DecryptUserData(authOTP, otpFile.RandomString);

            if (decryptedData is null)
                return false;

            (string username, string password) = otpManager.GetUserData(decryptedData);

            IPAddress? iPAddress = _httpContextAccessor?.HttpContext?.Connection.RemoteIpAddress;
            if (iPAddress == null)
                return false;

            string ipOfRequest = iPAddress.ToString();
            DBConnector connector = new(username, password, uuid, ipOfRequest);
            return connector.Client != null;
        }

        public bool IsAdmin()
        {
            string? username = GetUsername();
            if (username == null)
                return false;
            return _configManager.ReadKey("AdminUsername") == username;
        }

        public string? GetUsername()
        {
            (string? uuid, string? authOTP) = ReadOTPCookie();
            if (uuid is null || authOTP is null)
                return null;

            OTPFileObject? otpFile = _otpFileManagement.ReadOTPFile(uuid);
            if (otpFile is null)
                return null;

            OTPManagement otpManager = new();
            string? decryptedData = otpManager.DecryptUserData(authOTP, otpFile.RandomString);
            if (decryptedData is null)
                return null;

            (string username, string password) = otpManager.GetUserData(decryptedData);
            return username;
        }

        public (string?, string?) ReadOTPCookie()
        {
            string? uuid = _httpContextAccessor?.HttpContext?.Request.Cookies["UUID"];
            string? authOTP = Base64Decode(_httpContextAccessor?.HttpContext?.Request.Cookies["Token"]);
            return (uuid, authOTP);
        }

        public string? Base64Decode(string? base64EncodedData)
        {
            if (base64EncodedData is null)
                return null;

            var base64EncodedBytes = Convert.FromBase64String(base64EncodedData);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        }

        public string? GetUUID()
        {
            return _httpContextAccessor?.HttpContext?.Request.Cookies["UUID"];
        }
    }
}
