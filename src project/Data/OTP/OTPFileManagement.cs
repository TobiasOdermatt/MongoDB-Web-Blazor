using BlazorServerMyMongo.Data.Helpers;
using BlazorServerMyMongo.Objects;
using System.Text.Json;
using static BlazorServerMyMongo.Data.Helpers.LogManager;

namespace BlazorServerMyMongo.Data.OTP
{
    public class OTPFileManagement
    {
        string OTPpath = $"{Directory.GetCurrentDirectory()}" + @"\OTP\";
        const int CleanUpRefreshRateinDays = 1;
        const int DeleteOTPinDays = 10;

        public void WriteOTPFile(string uuid, OTPFileObject data)
        {
            string path = OTPpath + uuid + ".txt";
            string json = JsonSerializer.Serialize(data);
            File.WriteAllText(path, json);
        }

        public OTPFileObject? ReadOTPFile(string uuid)
        {
            string path = OTPpath + uuid + ".txt";
            if (!File.Exists(path))
                return null;

            string text = File.ReadAllText(path);
            OTPFileObject? otpfile = Newtonsoft.Json.JsonConvert.DeserializeObject<OTPFileObject>(text);
            if (otpfile is not null)
            {
                otpfile.LastAccess = DateTime.Now;
                return otpfile;
            }

            return null;
        }

        public void DeleteOTPFile(string? uuid)
        {
            if (uuid is null)
                return;

            string path = OTPpath + uuid + ".txt";
            if (File.Exists(path))
                File.Delete(path);
            LogManager log = new(LogType.Info, "Deleted OTP file Logout: " + uuid);
        }

        public void CleanUpOTPFiles()
        {
            if (!CheckCleanUpNeeded())
                return;

            UpdateCleanUpLog();
            LogManager log = new(LogType.Info, "Cleaned up OTP files");
            foreach (string fileName in Directory.GetFiles(OTPpath))
            {
                string text = File.ReadAllText(fileName);
                if (Path.GetFileName(fileName) == "CleanUpFile.txt")
                    continue;

                OTPFileObject? otpfile = Newtonsoft.Json.JsonConvert.DeserializeObject<OTPFileObject>(text);
                if (otpfile is not null)
                    if (otpfile.Created.AddDays(DeleteOTPinDays) < DateTime.Now)
                    {
                        File.Delete(fileName);
                        log = new(LogType.Info, "The OTP file " + fileName + " was deleted because it was older than " + DeleteOTPinDays + " days");
                    }
            }
        }

        private bool CheckCleanUpNeeded()
        {
            string CleanUpPath = OTPpath + "CleanUpFile.txt";
            if (File.Exists(CleanUpPath))
            {
                string text = File.ReadAllText(CleanUpPath);
                CleanUpFileObject? cleanUpFile = Newtonsoft.Json.JsonConvert.DeserializeObject<CleanUpFileObject>(text);
                if (cleanUpFile is not null)
                    if (cleanUpFile.LastCleanUp.AddDays(CleanUpRefreshRateinDays) < DateTime.Now)
                        return true;
            }
            else
                return true;

            return false;
        }

        private void UpdateCleanUpLog()
        {
            CleanUpFileObject data = new();
            data.LastCleanUp = DateTime.Now;
            string json = JsonSerializer.Serialize(data);
            File.WriteAllText(OTPpath + "CleanUpFile.txt", json);
        }
    }
}
