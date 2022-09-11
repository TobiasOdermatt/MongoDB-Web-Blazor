using MongoDB.Bson;
using MongoDB.Driver;

namespace BlazorServerMyMongo.Data.DB
{
    public class DBController
    {
        public static MongoClient? Client;

        public DBController() { }

        public DBController(MongoClient db)
        {
            Client = db;
        }

        //List every DB
        public List<BsonDocument>? ListAllDatabases()
        {
            if (Client is null)
                return null;
            var DBList = Client.ListDatabases();
            return DBList.ToList();
        }

        //List all collections from a DB catch if not authorized
        public List<BsonDocument>? ListAllCollectionsfromDB(string dbName)
        {
            if (Client is null)
                return null;
            try
            {
                return Client.GetDatabase(dbName).ListCollections().ToList();
            }
            catch
            {
                return null;
            }
        }

        //Get number of collections of a DB if not authorized return -1 
        public int GetNumberOfCollections(string dbName)
        {
            if (Client is null)
                return -1;
            try
            {
                return Client.GetDatabase(dbName).ListCollections().ToList().Count;
            }
            catch
            {
                return -1;
            }
        }

        //Get a specific collection from a DB based on the collectionName return JOBject


        //Delete DB by name
        public bool DeleteDB(string dbName)
        {
            if (Client is null)
                return false;
            try
            {
                Client.DropDatabase(dbName);
                return true;
            }
            catch
            {
                return false;
            }
        }

        //Create a Collection by name, and dbName
        public bool CreateCollection(string dbName, string collectionName)
        {
            if (Client is null)
                return false;
            try
            {
                Client.GetDatabase(dbName).CreateCollection(collectionName);
                return true;
            }
            catch
            {
                return false;
            }
        }

        //Delete Collection by name, and dbName
        public bool DeleteCollection(string dbName, string collectionName)
        {
            if (Client is null)
                return false;
            try
            {
                var db = Client.GetDatabase(dbName);
                db.DropCollection(collectionName);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
