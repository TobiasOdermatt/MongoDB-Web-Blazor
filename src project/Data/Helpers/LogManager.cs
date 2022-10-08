namespace BlazorServerMyMongo.Data.Helpers
{
    public class LogManager
    {
        readonly string path = $"{Directory.GetCurrentDirectory()}\\Logs\\{DateTime.Now.ToString("yyyy")}\\{DateTime.Now.ToString("MM")}\\";


        public LogManager(LogType type, string message)
        {
            CreateDirectory();
            CreateLogFile();
            UpdateLogFile(type, message);
        }

        public LogManager(LogType type, string message, Exception exception)
        {
            CreateDirectory();
            CreateLogFile();
            UpdateLogFile(type, message);
            CreateExceptionLogFile(message, exception);
        }

        public LogManager()
        {
            CreateDirectory();
            CreateLogFile();
        }

        public enum LogType
        {
            Info,
            Warning,
            Error
        }

        //Create the Exception File for the error
        private void CreateExceptionLogFile(string message, Exception exception)
        {
            string logMessage = DateTime.Now.ToString("HH:mm:ss") + " - " + message + exception.Message + " - " + exception.StackTrace;
            using (StreamWriter sw = new(path + "Exception.txt", true))
            {
                sw.WriteLine(logMessage);
                sw.Flush();
                sw.Close();
            }
        }

        //Create Start of Log File
        public void CreateLogFile()
        {
            if (!File.Exists(path + "Log.txt"))
            {
                StreamWriter CreateFile = new(path + "Log.txt", false);
                CreateFile.WriteLine("===============================");
                CreateFile.WriteLine("Web MongoDB Log");
                CreateFile.WriteLine("===============================");
                CreateFile.Flush();
                CreateFile.Close();
            }
        }

        //Add a new line to the log file
        public void UpdateLogFile(LogType type, string line)
        {
            try
            {
                StreamWriter file = new(path + "Log.txt", true);
                string logline = DateTime.Now.ToString("dd") + " - " + DateTime.Now.ToString("HH:mm:ss ") + "[" + type.ToString() + "]" + ": " + line;
                file.WriteLine(logline);
                Console.WriteLine(logline);
                file.Flush();
                file.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine($"[LogManager][UpdateLogFile] Error: {e.Message}");
            }
        }

        //Create Directoy if the Dir not exists.
        private void CreateDirectory()
        {
            try
            {
                DirectoryInfo dir = new(path);

                if (!dir.Exists)
                    dir.Create();
            }
            catch (Exception e)
            {
                Console.WriteLine($"[LogManager][CreateDirectory] Error: {e.Message}");
            }
        }
    }
}
