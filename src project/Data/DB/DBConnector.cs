﻿using BlazorServerMyMongo.Data.Helpers;
using MongoDB.Driver;

namespace BlazorServerMyMongo.Data.DB
{
    public class DBConnector
    {
        public MongoClient? Client;
        private static string? _dbHost;
        private static string? _dbPort;
        private static string? _dbRules;
        private static string? _customString;
        private static bool _useAuthorization = true;

        //Loads the connection values from config.json
        public DBConnector(IConfiguration config)
        {
            if (Boolean.TryParse(config["UseLogin"], out bool _useAuthorizationbool))
            {
                _useAuthorization = _useAuthorizationbool;
            }
            _dbHost = config["DBHost"];
            _dbPort = config["DBPort"];
            _dbRules = config["DBRule"];
            _customString = config["CustomConnectionString"];
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
<<<<<<< Updated upstream
                LogManager log = new("Error", "User: " + username + " has failed to connect to the DB ", e);
=======
                LogManager _ = new(LogManager.LogType.Error, "User; " + username + " has failed to connect to the DB ", e);
>>>>>>> Stashed changes
                return null;
            }
        }

        private string getConnectionString(string username, string password)
        {
            if (_customString != "" && _customString is not null) { return _customString; }

            if (_useAuthorization)
            {
                return $"mongodb://{username}:{password}@{_dbHost}:{_dbPort}/{_dbRules}";
            }
            return $"mongodb://{_dbHost}:{_dbPort}";
        }
    }

}
