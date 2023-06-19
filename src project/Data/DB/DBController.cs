using MongoDB_Web.Data.Helpers;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using static MongoDB_Web.Data.Helpers.LogManager;
using Newtonsoft.Json.Linq;
using Bogus;

namespace MongoDB_Web.Data.DB
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

        /// <summary>
        /// List every database from mongo
        /// </summary>
        /// <returns>Returns List<BsonDocument></returns>
        public List<BsonDocument>? ListAllDatabases()
        {
            if (Client is null)
                return null;

            List<BsonDocument>? dbList = new();
            try
            {
                dbList = Client.ListDatabases().ToList();
            }
            catch (Exception e)
            {
                LogManager _ = new(LogType.Error, "User: " + Username + " has failed to load Dashboard ", e);
                return null;
            }

            return dbList;
        }

        /// <summary>
        /// List every Collection from specific database
        /// </summary>
        /// <param name="dbName">Database Name</param>
        /// <returns>Returns List<BsonDocument></returns>
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
                LogManager _ = new(LogType.Error, "User: " + Username + " has failed to load the All Collections from DB: " + dbName, e);
                result = null;
            }

            return result;
        }

        /// <summary>
        /// Get the number of Collections
        /// </summary>
        /// <param name="dbName">Database Name</param>
        /// <returns>If the result is -1, user is not authenticated</returns>
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

        /// <summary>
        /// Get a specific Collection from a Database
        /// </summary>
        /// <param name="dbName">Database Name</param>
        /// <param name="collectionName">Collection Name</param>
        /// <returns>Returns List<String>?</returns>
        public List<string>? GetCollection(string dbName, string collectionName)
        {
            if (Client is null)
                return null;

            List<string> result = new();
            try
            {
                var collection = Client.GetDatabase(dbName).GetCollection<BsonDocument>(collectionName);

                if (collection is null)
                    return null;

                var filter = new BsonDocument();
                var cursor = collection.Find(filter).ToCursor();
                while (cursor.MoveNext())
                {
                    foreach (var document in cursor.Current)
                    {
                        result.Add(document.ToJson());
                    }
                }
                return result;
            }
            catch (Exception e)
            {
                LogManager _ = new(LogType.Error, "User: " + Username + " has failed to load the Collection: " + collectionName + " from DB: " + dbName, e);
                return null;
            }
        }

        public JObject? GetCollectionExport(string dbName, string collectionName)
        {
            if (Client is null)
                return null;

            try
            {
                var db = Client.GetDatabase(dbName);
                var collections = db.ListCollections().ToList();
                JObject result = new JObject();
                JObject databaseObject = new JObject();

                var collectionData = db.GetCollection<BsonDocument>(collectionName);
                var filter = new BsonDocument();
                var cursor = collectionData.Find(filter).ToCursor();
                JArray collectionArray = new JArray();
                var jsonWriterSettings = new JsonWriterSettings { OutputMode = JsonOutputMode.CanonicalExtendedJson };

                while (cursor.MoveNext())
                {
                    foreach (var document in cursor.Current)
                    {
                        JObject docAsJson = JObject.Parse(document.ToJson(jsonWriterSettings));
                        collectionArray.Add(docAsJson);
                    }
                }
                databaseObject.Add(collectionName, collectionArray);

                result.Add(dbName, databaseObject);
                return result;
            }
            catch (Exception e)
            {
                LogManager _ = new(LogType.Error, "User: " + Username + " has failed to load the Collection: " + collectionName + " from DB: " + dbName, e);
                return null;
            }
        }

        /// <summary>
        /// Get all collections from a Database, the return has every data from every collection in the DB
        /// </summary>
        /// <param name="dbName">Database Name</param>
        /// <returns>Returns JObject?</returns>
        public JObject? GetAllCollectionExport(string dbName)
        {
            if (Client is null)
                return null;

            try
            {
                var db = Client.GetDatabase(dbName);
                var collections = db.ListCollections().ToList();
                JObject result = new JObject();
                JObject databaseObject = new JObject();

                foreach (var collection in collections)
                {
                    var collectionName = collection["name"].AsString;
                    var collectionData = db.GetCollection<BsonDocument>(collectionName);
                    var filter = new BsonDocument();
                    var cursor = collectionData.Find(filter).ToCursor();
                    JArray collectionArray = new JArray();
                    var jsonWriterSettings = new JsonWriterSettings { OutputMode = JsonOutputMode.CanonicalExtendedJson };

                    while (cursor.MoveNext())
                    {
                        foreach (var document in cursor.Current)
                        {
                            JObject docAsJson = JObject.Parse(document.ToJson(jsonWriterSettings));
                            collectionArray.Add(docAsJson);
                        }
                    }
                    databaseObject.Add(collectionName, collectionArray);
                }

                result.Add(dbName, databaseObject);
                return result;
            }
            catch (Exception e)
            {
                LogManager _ = new(LogType.Error, "User: " + Username + " has failed to load the All Collections from DB: " + dbName, e);
                return null;
            }
        }

        /// <summary>
        /// Delete a specific Collection from a Database
        /// </summary>
        /// <param name="dbName">Database Name</param>
        /// <returns>bool will be returned, if success</returns>
        public bool DeleteDB(string dbName)
        {
            if (Client is null)
                return false;

            bool result;
            try
            {
                Client.DropDatabase(dbName);
                LogManager _ = new(LogType.Info, "User: " + Username + " has deleted the DB: " + dbName);
                result = true;
            }
            catch (Exception e)
            {
                LogManager _ = new(LogType.Error, "User: " + Username + " has failed to delete the DB: " + dbName + " " + e);
                result = false;
            }

            return result;
        }

        ///<summary>
        ///Check if datbase already exist
        /// </summary>
        /// <param name="dbName">Database name</param>
        /// <returns>bool will be returned</returns>
        public bool CheckIfDBExist(string dbName)
        {
            if (Client is null)
                return false;

            bool result;
            try
            {
                var filter = new BsonDocument("name", dbName);
                var options = new ListDatabasesOptions { Filter = filter };
                var list = Client.ListDatabases(options).ToList();
                if (list.Count == 0)
                {
                    result = false;
                }
                else
                {
                    result = true;
                }
            }
            catch (Exception e)
            {
                LogManager _ = new(LogType.Error, "User: " + Username + " has failed to check if DB: " + dbName + " exist", e);
                result = false;
            }

            return result;
        }
        /// <summary>
        /// Create a new Collection
        /// </summary>
        /// <param name="dbName">Database name</param>
        /// <param name="collectionName">Collection name</param>
        /// <returns>bool will be returned, if success</returns>
        public bool CreateCollection(string dbName, string collectionName)
        {
            if (Client is null)
                return false;

            bool result;
            try
            {
                Client.GetDatabase(dbName).CreateCollection(collectionName);
                LogManager _ = new(LogType.Info, "User: " + Username + " has created the Collection: " + collectionName + " in DB: " + dbName);
                result = true;
            }
            catch (Exception e)
            {
                LogManager _ = new(LogType.Error, "User: " + Username + " has failed by creating Collection " + collectionName + " in DB: " + dbName, e);
                result = false;
            }

            return result;
        }

        /// <summary>
        /// Delete a specific Collection
        /// </summary>
        /// <param name="dbName">Database Name</param>
        /// <param name="collectionName">Collection Name</param>
        /// <returns>bool will be returned, if success</returns>
        public bool DeleteCollection(string dbName, string collectionName)
        {
            if (Client is null)
                return false;

            bool result;
            try
            {
                var db = Client.GetDatabase(dbName);
                db.DropCollection(collectionName);
                LogManager _ = new(LogType.Info, "User: " + Username + " has deleted the Collection: " + collectionName + " in DB: " + dbName);
                result = true;
            }
            catch (Exception e)
            {
                LogManager _ = new(LogType.Error, "User: " + Username + " has failed by deleting Collection" + collectionName + " in DB: " + dbName, e);
                result = false;
            }

            return result;
        }

        public bool UploadJSON(string dbName, string collectionName, JToken json, bool adaptOid)
        {
            if (Client is null)
                return false;

            try
            {
                var db = Client.GetDatabase(dbName);
                var collection = db.GetCollection<BsonDocument>(collectionName);

                if (json is JArray jsonArray)
                {
                    foreach (JObject jsonObject in jsonArray)
                    {
                        if (!adaptOid && jsonObject.ContainsKey("_id"))
                        {
                            jsonObject.Remove("_id");
                        }

                        var document = BsonDocument.Parse(jsonObject.ToString());
                        collection.InsertOne(document);
                    }
                }
                else
                {
                    JObject jobject = JObject.Parse(json.ToString());
                    if (!adaptOid && jobject.ContainsKey("_id"))
                    {
                        jobject.Remove("_id");
                    }

                    var document = BsonDocument.Parse(jobject.ToString());
                    collection.InsertOne(document);
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task GenerateRandomData(string dbName, int collectionsCount, int totalDocuments)
        {
            if (Client is null)
                return;

            var faker = new Faker();

            var db = Client.GetDatabase(dbName);

            int documentsPerCollection = totalDocuments / collectionsCount;

            for (int i = 1; i <= collectionsCount; i++)
            {
                var collectionName = $"collection-{i}";
                var collection = db.GetCollection<BsonDocument>(collectionName);

                for (int j = 0; j < documentsPerCollection; j++)
                {
                    var randomData = new BsonDocument
                {
                    { "name", faker.Name.FullName()},
                    { "email", faker.Internet.Email()},
                    { "address", faker.Address.StreetAddress()},
                    { "phone", faker.Phone.PhoneNumber()},
                    { "company", faker.Company.CompanyName()},
                    { "jobTitle", faker.Name.JobTitle()},
                };

                    await collection.InsertOneAsync(randomData);
                }

                Console.WriteLine($"{collectionName} mit {documentsPerCollection} Dokumenten erstellt.");
            }
        }

    public byte[] ConvertToBson(List<string> documents)
        {
            var bsonDocuments = new List<BsonDocument>();

            foreach (var document in documents)
            {
                bsonDocuments.Add(BsonDocument.Parse(document));
            }

            var memoryStream = new MemoryStream();
            var bsonWriter = new BsonBinaryWriter(memoryStream);
            var context = BsonSerializationContext.CreateRoot(bsonWriter);
            var documentSerializer = new BsonDocumentSerializer();
            documentSerializer.Serialize(context, bsonDocuments);

            return memoryStream.ToArray();
        }
    }
}
