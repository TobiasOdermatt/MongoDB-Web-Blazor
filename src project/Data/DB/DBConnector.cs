using MongoDB_Web.Data.Helpers;
using MongoDB.Driver;
using static MongoDB_Web.Data.Helpers.LogManager;

namespace MongoDB_Web.Data.DB
{
    public class DBConnector
    {
        public MongoClient? Client;
        static string? dbHost;
        static string? dbPort;
        static string? dbRules;
        static string? customString;
        static bool useAuthorization = true;

        //Loads the connection values from config.json
        public DBConnector(IConfiguration config)
        {
            if (Boolean.TryParse(config["UseLogin"], out bool _useAuthorizationbool))
            {
                useAuthorization = _useAuthorizationbool;
            }
            dbHost = config["DBHost"];
            dbPort = config["DBPort"];
            dbRules = config["DBRule"];
            customString = config["CustomConnectionString"];
        }

        public DBConnector(string username, string password, string uuid, string ipOfRequest)
        {
            Client = DbConnect(username, password, uuid, ipOfRequest);
        }

        public MongoClient? DbConnect(string username, string password, string uuid, string ipOfRequest)
        {
            string connectionString = getConnectionString(username, password);
            if (DBController.UUID == uuid && DBController.IPofRequest == ipOfRequest)
                return DBController.Client;

            try
            {
                MongoClient mongoClient = new(connectionString);
                DBController dBController = new(mongoClient, uuid, username, ipOfRequest);
                return mongoClient;
            }
            catch (Exception e)
            {
                LogManager _ = new(LogType.Error, "User; " + username + " has failed to connect to the DB ", e);
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
    }

}
