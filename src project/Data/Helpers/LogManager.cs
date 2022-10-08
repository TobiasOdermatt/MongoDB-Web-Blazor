namespace BlazorServerMyMongo.Data.Helpers
{
    public class LogManager
    {
        readonly string path = $"{Directory.GetCurrentDirectory()}\\Logs\\{DateTime.Now.ToString("yyyy")}\\{DateTime.Now.ToString("MM")}\\";


        /// <summary>
        /// Write a new log
        /// </summary>
        /// <param name="type"></param>
        /// <param name="message"></param>
        public LogManager(LogType type, string message)
        {
            CreateDirectory();
            CreateLogFile(type);
            UpdateLogFile(type, message);
        }

        /// <summary>
        /// Create a new Log with Exception
        /// </summary>
        /// <param name="type"></param>
        /// <param name="message"></param>
        /// <param name="exception"></param>
        public LogManager(LogType type, string message, Exception exception)
        {
            CreateDirectory();
            CreateLogFile(type);
            UpdateLogFile(type, message);
            CreateExceptionLogFile(message, exception);
        }

        public LogManager()
        {
            CreateDirectory();
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
        public void CreateLogFile(LogType type)
        {
            if (!File.Exists(path + type + ".txt"))
            {
                StreamWriter CreateFile = new(path + type + ".txt", false);
                CreateFile.WriteLine("===============================");
                CreateFile.WriteLine("Web MongoDB Log " + type);
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
                StreamWriter file = new(path + type + ".txt", true);
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
