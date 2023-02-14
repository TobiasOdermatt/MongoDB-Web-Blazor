using BlazorServerMyMongo.Objects;
namespace BlazorServerMyMongo.Data.Helpers
{
    public class LogManager
    {

        static string currentDirectory = $"{Directory.GetCurrentDirectory()}";
        readonly string path = $"{currentDirectory}\\Logs\\{DateTime.Now.ToString("yyyy")}\\{DateTime.Now.ToString("MM")}\\";



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
                CreateFile.WriteLine("#===============================");
                CreateFile.WriteLine("# Web MongoDB Log " + type);
                CreateFile.WriteLine("#===============================");
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
                string logline = DateTime.Now.ToString("dd") + " | " + DateTime.Now.ToString("HH:mm:ss ") + "|[" + type.ToString() + "]|" + "| " + line;
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

        public (int, int, int) CountLog(DateTime date)
        {
            string currentpath = $"{currentDirectory}\\Logs\\{date.ToString("yyyy")}\\{date.ToString("MM")}\\";
            int countOfInfo = CountLinesInLogFile(currentpath + "Info.txt");
            int countOfWarning = CountLinesInLogFile(currentpath + "Warning.txt");
            int countOfError = CountLinesInLogFile(currentpath + "Error.txt");
            return (countOfInfo, countOfWarning, countOfError);
        }

        // Counts the lines in the log file -3
        private int CountLinesInLogFile(string filePath)
        {
            int count = 0;
            try
            {
                if (File.Exists(filePath))
                {
                    StreamReader file = new(filePath);
                    while (file.ReadLine() != null)
                        count++;
                    file.Close();
                    count -= 3;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"[LogManager][CountLinesInLogFile] Error: {e.Message}");
            }
            return count;
        }

        public List<DateTime> GetAvailableLogDates()
        {
            List<DateTime> dates = new();
            try
            {
                DirectoryInfo dir = new(currentDirectory + "\\Logs");
                foreach (var year in dir.GetDirectories())
                {
                    foreach (var month in year.GetDirectories())
                    {
                        DateTime date = new(Int32.Parse(year.Name), Int32.Parse(month.Name), 1);
                        dates.Add(date);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"[LogManager][GetLogDates] Error: {e.Message}");
            }
            return dates;
        }

        public List<LogObject> ReadSingleLogFile(string path, DateTime dateTime)
        {
            List<LogObject> logs = new();
            try
            {
                if (File.Exists(path))
                {
                    StreamReader file = new(path);
                    string line;
                    while ((line = file.ReadLine()) != null)
                    {
                        //If line start with # then skip
                        if (line.StartsWith("#"))
                            continue;
                        
                        string[] log = line.Split(" | ");
                        string[] date = log[0].Split(".");
                        string[] time = log[1].Split(" ");
                        string[] type = time[1].Split("]|");
                        string[] message = log[1].Split("||");
                        
                        LogObject logObject = new()
                        {
                            Created = dateTime.Add(new TimeSpan(Int32.Parse(time[0].Split(":")[0]), Int32.Parse(time[0].Split(":")[1]), Int32.Parse(time[0].Split(":")[2]))),
                            Type = type[0].Replace("|[", ""),
                            Message = message[1]
                        };
                        logs.Add(logObject);
                    }
                    file.Close();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"[LogManager][ReadSingleLogFile] Error: {e.Message}");
            }
            return logs;
        }
    

        //Read Log Files form Argument LogType and DateTime if type is All read all Log Files and return a List of LogObjects
        public List<LogObject> ReadLogFiles(string type, DateTime date)
        {
            List<LogObject> logObjects = new();
            string currentpath = $"{currentDirectory}\\Logs\\{date.ToString("yyyy")}\\{date.ToString("MM")}\\";
            try
            {
                if (type == "Info")
                {
                    logObjects = ReadSingleLogFile(currentpath + "Info.txt", date);
                }
                else if (type == "Warning")
                {
                    logObjects = ReadSingleLogFile(currentpath + "Warning.txt", date);
                }
                else if (type == "Error")
                {
                    logObjects = ReadSingleLogFile(currentpath + "Error.txt", date);
                }
                else
                {
                    logObjects.AddRange(ReadSingleLogFile(currentpath + "Info.txt", date));
                    logObjects.AddRange(ReadSingleLogFile(currentpath + "Warning.txt", date));
                    logObjects.AddRange(ReadSingleLogFile(currentpath + "Error.txt", date));
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"[LogManager][ReadLogFiles] Error: {e.Message}");
            }
            return logObjects;
        }
    }
}
