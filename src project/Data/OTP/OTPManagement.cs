using BlazorServerMyMongo.Objects;
using System.Text;

namespace BlazorServerMyMongo.Data.OTP
{
    public class OTPManagement
    {
        public string? DecryptUserData(string? AuthCookieKey, string? RandData)
        {
            if (AuthCookieKey is null || RandData is null)
                return null;

            string DecryptedData = BinaryStringToText(XORBinary(AuthCookieKey, RandData));

            if (!DecryptedData.Contains("Data:"))
                return null;

            return DecryptedData;
        }

        public (string, string) getUserData(string data)
        {
            StringBuilder builder = new StringBuilder(data);
            builder.Replace("Data:", "");
            string[] DataArray = builder.ToString().Split("@");

            if (DataArray.Length is not 2)
                return ("", "");

            return (DataArray[0], DataArray[1]);
        }
        
        private static string XORBinary(string bin1, string bin2)
        {
            int len = Math.Max(bin1.Length, bin2.Length);
            string res = "";
            bin1 = bin1.PadLeft(len, '0');
            bin2 = bin2.PadLeft(len, '0');
            for (int i = 0; i < len; i++)
            {
                if (bin1[i] == ' ')
                { res += ' '; continue; }

                res += bin1[i] == bin2[i] ? '0' : '1';
            }
            return res;
        }

        private string BinaryStringToText(string binary)
        {
            string[] binaryArray = binary.Split(' ');
            string text = "";
            foreach (string s in binaryArray)
            {
                int i = Convert.ToInt32(s, 2);
                char c = (char)i;
                text += c.ToString();
            }
            return text;
        }
    }
}
