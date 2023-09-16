using MongoDB_Web.Data.Helpers;
using MongoDB_Web.Objects;
using System.Text.Json;
using static MongoDB_Web.Data.Helpers.LogManager;

namespace MongoDB_Web.Data.OTP
{
    public class OTPFileManagement
    {
        string otppath = $"{Directory.GetCurrentDirectory()}" + @"\OTP\";
        string userStoragePath = $"{Directory.GetCurrentDirectory()}" + @"\UserStorage\";
        const int CLEANUP_FRESHRATE_IN_DAY = 1;
        const int DELETE_OTP_IN_DAYS = 10;

        public void WriteOTPFile(string uuid, OTPFileObject data)
        {
            string path = otppath + uuid + ".txt";
            string json = JsonSerializer.Serialize(data);
            File.WriteAllText(path, json);
        }

        public OTPFileObject? ReadOTPFile(string uuid)
        {
            string path = otppath + uuid + ".txt";
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

            string path = otppath + uuid + ".txt";
            string userpath = userStoragePath + uuid;
            if (File.Exists(path))
                File.Delete(path);

            if (Directory.Exists(userpath))
                DeleteDirectory(userpath);

            LogManager _ = new(LogType.Info, "Deleted OTP file Logout: " + uuid);
        }

        public void CleanUpOTPFiles()
        {
            if (!CheckCleanUpNeeded())
                return;

            UpdateCleanUpLog();
            LogManager log = new(LogType.Info, "Cleaned up OTP files");
            foreach (string fileName in Directory.GetFiles(otppath))
            {
                string text = File.ReadAllText(fileName);
                if (Path.GetFileName(fileName) == "CleanUpFile.txt")
                    continue;

                OTPFileObject? otpfile = Newtonsoft.Json.JsonConvert.DeserializeObject<OTPFileObject>(text);
                if (otpfile is not null)
                    if (otpfile.Created.AddDays(DELETE_OTP_IN_DAYS) < DateTime.Now)
                    {
                        File.Delete(fileName);
                        log = new(LogType.Info, "The OTP file " + fileName + " was deleted because it was older than " + DELETE_OTP_IN_DAYS + " days");
                        string uuid = Path.GetFileNameWithoutExtension(fileName);
                        string userpath = userStoragePath + uuid;
                        if (Directory.Exists(userpath))
                            DeleteDirectory(userpath);
                    }
            }
        }

        bool CheckCleanUpNeeded()
        {
            if (!Directory.Exists(otppath))
                Directory.CreateDirectory(otppath);

            string cleanUpPath = otppath + "CleanUpFile.txt";
            if (File.Exists(cleanUpPath))
            {
                string text = File.ReadAllText(cleanUpPath);
                CleanUpFileObject? cleanUpFile = Newtonsoft.Json.JsonConvert.DeserializeObject<CleanUpFileObject>(text);
                if (cleanUpFile is not null)
                    if (cleanUpFile.LastCleanUp.AddDays(CLEANUP_FRESHRATE_IN_DAY) < DateTime.Now)
                        return true;
            }
            else
                return true;

            return false;
        }

        void UpdateCleanUpLog()
        {
            CleanUpFileObject data = new()
            {
                LastCleanUp = DateTime.Now
            };

            string json = JsonSerializer.Serialize(data);
            if (!File.Exists(otppath + "CleanUpFile.txt"))
                File.Create(otppath + "CleanUpFile.txt");

            File.WriteAllText(otppath + "CleanUpFile.txt", json);
        }

        public static void DeleteDirectory(string target_dir)
        {
            string[] files = Directory.GetFiles(target_dir);
            string[] dirs = Directory.GetDirectories(target_dir);

            foreach (string file in files)
            {
                File.SetAttributes(file, FileAttributes.Normal);
                File.Delete(file);
            }

            foreach (string dir in dirs)
            {
                DeleteDirectory(dir);
            }

            Directory.Delete(target_dir, false);
        }
    }
}
