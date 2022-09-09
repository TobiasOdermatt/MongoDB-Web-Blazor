using MongoDB.Bson;
using MongoDB.Driver;

namespace BlazorServerMyMongo.Data.DB
{
    public class DBConnector
    {
        public static MongoClient? Client;
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

        public DBConnector(string username, string password)
        {
            Client = DbConnect(username, password);
        }

        public MongoClient? DbConnect(string username, string password)
        {
            string ConnectionString = getConnectionString(username, password);

            try
            {
                MongoClient mongoClient = new(ConnectionString);

                var testDB = mongoClient.GetDatabase("test");
                var cmd = new BsonDocument("count", "foo");
                var result = testDB.RunCommand<BsonDocument>(cmd);
                DBController dBController = new(mongoClient);
                return mongoClient;
            }
            catch (Exception)
            {
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
