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
        static string? UUID;

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

        public DBConnector(string username, string password, string UUID)
        {
            Client = DbConnect(username, password, UUID);
        }

        public MongoClient? DbConnect(string username, string password, string UUID)
        {
            string ConnectionString = getConnectionString(username, password);
            DBController controller = new();
            if (DBController.UUID == UUID)
                return DBController.Client;

            try
            {
                MongoClient mongoClient = new(ConnectionString);

                // var testDB = mongoClient.GetDatabase("test");
                // var cmd = new BsonDocument("count", "foo");
                // var result = testDB.RunCommand<BsonDocument>(cmd);
                var test = mongoClient.ListDatabases();
                DBController dBController = new(mongoClient, UUID);
                return mongoClient;
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: Connection to DB failed \n" + e);
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
