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
                _ = new LogManager(LogType.Error, "User; " + username + " has failed to connect to the DB, IP: " + ipOfRequest + " is not allowed");
                return null;
            }

            string connectionString = getConnectionString(username, password);
            if (DBController.UUID == uuid && DBController.IPofRequest == ipOfRequest)
                return DBController.Client;

            try
            {
                MongoClient mongoClient = new(connectionString);
                DBController dBController = new(mongoClient, uuid, username, ipOfRequest, batchCount);

                if (dBController.ListAllDatabases() == null)
                {
                    throw new Exception("Failed to list all databases");
                }
                else
                {
                    return mongoClient;
                }
            }
            catch (Exception e)
            {
                _ = new LogManager(LogType.Error, "User; " + username + " has failed to connect to the DB ", e);
                return null;
            }
        }

        string getConnectionString(string username, string password)
        {
            if (customString != "" && customString is not null) 
                 return customString; 

            if (useAuthorization)          
                return $"mongodb://{username}:{password}@{dbHost}:{dbPort}/{dbRules}";
            
            return $"mongodb://{dbHost}:{dbPort}";
        }


        /// <summary>
        /// Test if the user see any db
        /// </summary>
        /// <returns>If dbs ey</returns>
        public bool ListAllDatabasesTest()
        {
            try
            {
                List<BsonDocument>? dbList = new();
                dbList = Client?.ListDatabases().ToList();
                if(dbList == null)
                    return false;
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }
    }

}
