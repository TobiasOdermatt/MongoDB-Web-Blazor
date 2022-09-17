using BlazorServerMyMongo.Data.Helpers;
using MongoDB.Bson;
using MongoDB.Driver;

namespace BlazorServerMyMongo.Data.DB
{
    public class DBController
    {
        public static MongoClient? Client;
        public static string? UUID;
        public static string? Username;
        public static string? IPofRequest;

        public DBController() { }

        public DBController(MongoClient db, string uuid, string username, string ipOfRequest)
        {
            Client = db;
            UUID = uuid;
            Username = username;
            IPofRequest = ipOfRequest;
        }

        //List every DB
        public List<BsonDocument>? ListAllDatabases()
        {
            if (Client is null)
                return null;
            List<BsonDocument>? DBList = new();
            try
            {
                DBList = Client.ListDatabases().ToList();
            }
            catch (Exception e)
            {
                LogManager log = new("Error", "User: " + Username + " has failed to load Dashboard ", e);
            }
            return DBList;
        }

        //List all collections from a DB catch if not authorized
        public List<BsonDocument>? ListAllCollectionsfromDB(string dbName)
        {
            if (Client is null)
                return null;
            List<BsonDocument>? result;
            try
            {
                result = Client.GetDatabase(dbName).ListCollections().ToList();
            }
            catch (Exception e)
            {
                LogManager log = new("Error", "User: " + Username + " has failed to load the All Collections from DB: " + dbName, e);
                result = null;
            }
            return result;
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

        //Get a specific collection from a DB based on the collectionName return IMongoCollection<BsonDocument>
        public List<string>? GetCollection(string dbName, string collectionName)
        {
            if (Client is null)
                return null;
            List<string> result = new();
            try
            {
                var Collection = Client.GetDatabase(dbName).GetCollection<BsonDocument>(collectionName);

                if (Collection is null)
                    return null;

                var Filter = new BsonDocument();
                var Cursor = Collection.Find(Filter).ToCursor();
                while (Cursor.MoveNext())
                {
                    foreach (var Document in Cursor.Current)
                    {
                        result.Add(Document.ToJson());
                    }
                }
                return result;
            }
            catch (Exception e)
            {
                LogManager log = new("Error", "User: " + Username + " has failed to load the Collection: " + collectionName + " from DB: " + dbName, e);
            }
            return null;
        }

        //Delete DB by name
        public bool DeleteDB(string dbName)
        {
            if (Client is null)
                return false;
            bool result;
            try
            {
                Client.DropDatabase(dbName);
                LogManager log = new("Info", "User: " + Username + " has deleted the DB: " + dbName);
                result = true;
            }
            catch (Exception e)
            {
                LogManager log = new("Error", "User: " + Username + " has failed to delete the DB: " + dbName + " " + e);
                result = false;
            }
            return result;

        }

        //Create a Collection by name, and dbName
        public bool CreateCollection(string dbName, string collectionName)
        {
            if (Client is null)
                return false;
            bool result = false;
            try
            {
                Client.GetDatabase(dbName).CreateCollection(collectionName);
                LogManager log = new("Info", "User: " + Username + " has created the Collection: " + collectionName + " in DB: " + dbName);
                result = true;
            }
            catch (Exception e)
            {
                LogManager log = new("Error", "User: " + Username + " has failed by creating Collection " + collectionName + " in DB: " + dbName, e);
                result = false;
            }
            return result;
        }

        //Delete Collection by name, and dbName
        public bool DeleteCollection(string dbName, string collectionName)
        {
            bool result = false;
            if (Client is null)
                result = false;
            try
            {
                var db = Client.GetDatabase(dbName);
                db.DropCollection(collectionName);
                LogManager log = new("Info", "User: " + Username + " has deleted the Collection: " + collectionName + " in DB: " + dbName);
                result = true;
            }
            catch (Exception e)
            {
                LogManager log = new("Error", "User: " + Username + " has failed by deleting Collection" + collectionName + " in DB: " + dbName, e);
                result = false;
            }
            return result;
        }

        //Upload JSON to the collection by, dbName, collectionName, and the JSON
        public bool UploadJSON(string dbName, string collectionName, string json)
        {
            if (Client is null)
                return false;
            try
            {
                var db = Client.GetDatabase(dbName);
                var collection = db.GetCollection<BsonDocument>(collectionName);
                var document = BsonDocument.Parse(json);
                collection.InsertOne(document);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
