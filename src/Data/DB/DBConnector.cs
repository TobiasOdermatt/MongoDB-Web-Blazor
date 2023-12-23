using MongoDB_Web.Data.Helpers;
using MongoDB.Driver;
using static MongoDB_Web.Data.Helpers.LogManager;
using MongoDB_Web.Controllers;
using MongoDB.Bson;

namespace MongoDB_Web.Data.DB
{
    public class DBConnector
    {
        public MongoClient? Client;
        static string? dbHost;
        static string? dbPort;
        static string? dbRules;
        static string? customString;
        static string? allowedIp;
        static bool useAuthorization = true;
        static int batchCount = 90;

        //Loads the connection values from config.json
        public DBConnector(IConfiguration config)
        {
            if (Boolean.TryParse(config["UseAuthorization"], out bool _useAuthorizationbool))
            {
                useAuthorization = _useAuthorizationbool;
            }
            if (int.TryParse(config["BatchCount"], out int _batchCount))
            {
                batchCount = _batchCount;
            }

            dbHost = config["DBHost"];
            dbPort = config["DBPort"];
            dbRules = config["DBRule"];
            customString = config["CustomConnectionString"];
            allowedIp = config["AllowedIP"];
        }

        public DBConnector(string username, string password, string uuid, string ipOfRequest)
        {
            Client = DbConnect(username, password, uuid, ipOfRequest);
        }

        public MongoClient? DbConnect(string username, string password, string uuid, string ipOfRequest)
        {
            if (allowedIp != "*" && allowedIp != ipOfRequest)
            {
                log($"User; {username} has failed to connect to the DB, IP: {ipOfRequest} is not allowed");
                return null;
            }

            if (DBController.UUID == uuid && DBController.IPofRequest == ipOfRequest)
                return DBController.Client;

            string connectionString = getConnectionString(username, password);

            try
            {
                MongoClient mongoClient = new(connectionString);
                DBController dBController = new(mongoClient, uuid, username, ipOfRequest, batchCount);

                if (dBController.ListAllDatabases() != null)
                {
                    return mongoClient;
                }

                throw new Exception("Failed to list all databases");
            }
            catch (Exception e)
            {
                log($"User; {username} has failed to connect to the DB", e);
                return null;
            }
        }

        private void log(string message, Exception? e = null)
        {
            if(e == null)
                _ = new LogManager(LogType.Error, message);
            else
                _ = new LogManager(LogType.Error, message, e);
        }


        string getConnectionString(string username, string password)
        {
            if (!string.IsNullOrEmpty(customString))
                return customString;

            return useAuthorization
                ? $"mongodb://{Sanitize(username)}:{Sanitize(password)}@{dbHost}:{dbPort}/{dbRules}"
                : $"mongodb://{dbHost}:{dbPort}";
        }

        private static string Sanitize(string input)
        {
            input = input.Trim();
            return Uri.EscapeDataString(input);
        }


        /// <summary>
        /// Test if the user can see any databases.
        /// </summary>
        /// <returns>True if databases exist, otherwise false.</returns>
        public bool ListAllDatabasesTest()
        {
            try
            {
                return Client?.ListDatabases().Any() == true;
            }
            catch
            {
                return false;
            }
        }
    }

}
