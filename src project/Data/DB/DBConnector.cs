using BlazorServerMyMongo.Data.Helpers;
using MongoDB.Driver;
using static BlazorServerMyMongo.Data.Helpers.LogManager;

namespace BlazorServerMyMongo.Data.DB
{
    public class DBConnector
    {
        public MongoClient? Client;
        private static string? dbHost;
        private static string? dbPort;
        private static string? dbRules;
        private static string? customString;
        private static bool _useAuthorization = true;

        //Loads the connection values from config.json
        public DBConnector(IConfiguration config)
        {
            if (Boolean.TryParse(config["UseLogin"], out bool _useAuthorizationbool))
            {
                _useAuthorization = _useAuthorizationbool;
            }
            dbHost = config["DBHost"];
            dbPort = config["DBPort"];
            dbRules = config["DBRule"];
            customString = config["CustomConnectionString"];
        }

        public DBConnector(string username, string password, string uuid, string IPOfRequest)
        {
            Client = DbConnect(username, password, uuid, IPOfRequest);
        }

        public MongoClient? DbConnect(string username, string password, string uuid, string ipOfRequest)
        {
            string ConnectionString = getConnectionString(username, password);
            DBController controller = new();
            if (DBController.UUID == uuid && DBController.IPofRequest == ipOfRequest)
                return DBController.Client;

            try
            {
                MongoClient mongoClient = new(ConnectionString);
                var test = mongoClient.ListDatabases();
                DBController dBController = new(mongoClient, uuid, username, ipOfRequest);
                return mongoClient;
            }
            catch (Exception e)
            {
                LogManager _ = new(LogType.Error, "User: " + username + " has failed to connect to the DB ", e);
                return null;
            }
        }

        private string getConnectionString(string username, string password)
        {
            if (customString != "" && customString is not null) { return customString; }

            if (_useAuthorization)
            {
                return $"mongodb://{username}:{password}@{dbHost}:{dbPort}/{dbRules}";
            }
            return $"mongodb://{dbHost}:{dbPort}";
        }
    }

}
