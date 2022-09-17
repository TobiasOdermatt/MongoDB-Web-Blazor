using BlazorServerMyMongo.Data.DB;
using BlazorServerMyMongo.Objects;

namespace BlazorServerMyMongo.Data.OTP
{
    public class OTPAuthCookieManagement
    {
        public HttpContext Context { get; set; }

        public OTPAuthCookieManagement(HttpContext httpContext)
        {
            Context = httpContext;
        }

        //Checks if the AuthCookie is valid
        public bool IsCookieValid()
        {
            (string? uuid, string? authOTP) = ReadOTPCookie();
            if (uuid is null || authOTP is null)
                return false;

            OTPFileManagement fileManager = new();
            fileManager.CleanUpOTPFiles();
            OTPFileObject? otpFile = fileManager.ReadOTPFile(uuid);

            if (otpFile is null)
                return false;

            OTPManagement otpManager = new();
            string? decryptedData = otpManager.DecryptUserData(authOTP, otpFile.RandomString);

            if (decryptedData is null)
                return false;

            (string username, string password) = otpManager.getUserData(decryptedData);
            DBConnector connector = new(username, password, uuid, Context.Request.HttpContext.Connection.RemoteIpAddress.ToString());
            return connector.Client != null;
        }

        public (string?, string?) ReadOTPCookie()
        {
            string? uuid = Context.Request.Cookies["UUID"];
            string? authOTP = Base64Decode(Context.Request.Cookies["Token"]);
            return (uuid, authOTP);
        }

        public string? Base64Decode(string? base64EncodedData)
        {
            if (base64EncodedData is null)
                return null;

            var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        }
    }
}
