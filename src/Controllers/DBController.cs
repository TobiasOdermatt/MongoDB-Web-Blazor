﻿using MongoDB_Web.Data.Helpers;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Bson.Serialization;
using Newtonsoft.Json;
using MongoDB.Driver;
using static MongoDB_Web.Data.Helpers.LogManager;
using Newtonsoft.Json.Linq;
using Bogus;
using Microsoft.AspNetCore.SignalR;
using MongoDB_Web.Data.Hubs;


namespace MongoDB_Web.Controllers
{
    public class DBController
    {
        public static MongoClient? Client;
        public static string? UUID;
        public static string? Username;
        public static string? IPofRequest;
        public static int BatchCount;

        public readonly IHubContext<ProgressHub>? _hubContext;

        public DBController(IHubContext<ProgressHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public DBController() { Console.WriteLine("DBController loaded"); }

        public DBController(MongoClient db, string uuid, string username, string ipOfRequest, int batchCount)
        {
            Client = db;
            UUID = uuid;
            Username = username;
            IPofRequest = ipOfRequest;
            BatchCount = batchCount;
        }

        /// <summary>
        /// List every database from mongo
        /// </summary>
        /// <returns>Returns List<BsonDocument></returns>
        public List<BsonDocument>? ListAllDatabases()
        {
            List<BsonDocument>? dbList = new();
            try
            {
                dbList = Client?.ListDatabases().ToList();
            }
            catch (Exception e)
            {
                LogManager _ = new(LogType.Error, "User: " + Username + " has failed to load Dashboard ", e);
                return null;
            }

            return dbList;
        }

        /// <summary>
        /// Get all available attributes from a collection in a database.
        /// </summary>
        /// <param name="dbName">Database Name</param>
        /// <param name="collectionName">Collection Name</param>
        /// <returns>List of attribute names</returns>
        public List<string> GetCollectionAttributes(string? dbName, string? collectionName)
        {
            if (Client is null || dbName is null || collectionName is null)
                return new List<string>();

            List<string> keyList = new();

            try
            {
                var db = Client.GetDatabase(dbName);
                var collection = db.GetCollection<BsonDocument>(collectionName);

                var filter = new BsonDocument();
                var cursor = collection.Find(filter).Limit(1);

                if (cursor.Any())
                {
                    BsonDocument document = cursor.First();
                    foreach (var element in document.Elements)
                    {
                        keyList.Add(element.Name);
                    }
                }
            }
            catch (Exception e)
            {
                LogManager _ = new(LogType.Error, $"User: {Username} has failed to load attributes from Collection: {collectionName} in DB: {dbName}", e);
                return new List<string>();
            }
            return keyList;
        }

        /// <summary>
        /// Rename a collection in a database.
        /// </summary>
        /// <param name="dbName">Database Name</param>
        /// <param name="oldCollectionName">Old Collection Name</param>
        /// <param name="newCollectionName">New Collection Name</param>
        /// <returns>True if the collection was renamed successfully, false otherwise</returns>
        public async Task<bool> MoveCollection(string? oldDbName, string? newDbName, string? oldCollectionName, string? newCollectionName, Guid guid)
        {
            if (Client is null || oldDbName is null || newDbName is null || oldCollectionName is null || newCollectionName is null)
                return false;

            try
            {
                var oldDb = Client.GetDatabase(oldDbName);
                var newDb = Client.GetDatabase(newDbName);

                var oldCollection = oldDb.GetCollection<BsonDocument>(oldCollectionName);
                var newCollection = newDb.GetCollection<BsonDocument>(newCollectionName);

                var cursor = await oldCollection.FindAsync(FilterDefinition<BsonDocument>.Empty);
                List<BsonDocument> batch = new List<BsonDocument>();

                await cursor.ForEachAsync(async document =>
                {
                    batch.Add(document);
                    if (batch.Count >= BatchCount)
                    {
                        await newCollection.InsertManyAsync(batch);
                        batch.Clear();
                    }
                });

                if (batch.Count > 0)
                {
                    await newCollection.InsertManyAsync(batch);
                }
                await _hubContext!.Clients.All.SendAsync("ReceiveProgressCollection", guid.ToString(), "move");

                return true;
            }
            catch (Exception e)
            {
                LogManager _ = new(LogType.Error, $"Error while moving the collection {oldCollectionName} from {oldDbName} to db {newDbName} {newCollectionName}", e);
                return false;
            }
        }

        /// <summary>
        /// Move a database to a new Datbase
        /// </summary>
        /// <param name="oldDbName">Old Database Name</param>
        /// <param name="newDbName">New Database Name</param>
        /// <returns>True if the database was moved successfully, false otherwise</returns>
        public async Task<bool> MoveDatabase(string? oldDbName, string? newDbName, Guid guid)
        {
            if (Client is null || oldDbName is null || newDbName is null)
                return false;

            try
            {
                var oldDb = Client.GetDatabase(oldDbName);
                var newDb = Client.GetDatabase(newDbName);
                var collections = oldDb.ListCollectionNames().ToList();
                int totalCollections = collections.Count;

                foreach (var collectionName in collections)
                {
                    var oldCollection = oldDb.GetCollection<BsonDocument>(collectionName);
                    var newCollection = newDb.GetCollection<BsonDocument>(collectionName);

                    var cursor = await oldCollection.FindAsync(FilterDefinition<BsonDocument>.Empty);
                    List<BsonDocument> batch = new List<BsonDocument>();

                    await cursor.ForEachAsync(async document =>
                    {
                        batch.Add(document);
                        if (batch.Count >= BatchCount)
                        {
                            await newCollection.InsertManyAsync(batch);
                            batch.Clear();
                        }
                    });

                    if (batch.Count > 0)
                    {
                        await newCollection.InsertManyAsync(batch);
                    }

                    double progress = ((double)(collections.IndexOf(collectionName) + 1) / totalCollections) * 100;
                    await _hubContext!.Clients.All.SendAsync("ReceiveProgressDatabase", totalCollections, collections.IndexOf(collectionName) + 1, progress, guid.ToString(), "move");
                }
                return true;
            }
            catch (Exception e)
            {
                LogManager _ = new(LogType.Error, $"Error while moving the database from {oldDbName} to {newDbName}", e);
                return false;
            }
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

        public async Task<bool> RenameAttributeInCollectionAsync(string dbName, string collectionName, Dictionary<string, string> renameMap)
        {
            if (Client is null)
                return false;

            try
            {
                var db = Client.GetDatabase(dbName);
                var collection = db.GetCollection<BsonDocument>(collectionName);

                var updateDefs = renameMap.Select(entry =>
                    Builders<BsonDocument>.Update.Rename(entry.Key, entry.Value)
                ).ToArray();

                var combined = Builders<BsonDocument>.Update.Combine(updateDefs);

                await collection.UpdateManyAsync(new BsonDocument(), combined);

                return true;
            }
            catch (Exception e)
            {
                LogManager _ = new(LogType.Error, "User: " + Username + " has failed to change the attributes in " + collectionName + " from DB: " + dbName, e);
                return false;
            }
        }


        public long GetTotalCount(string dbName, string collectionName, string selectedKey, string searchValue)
        {
            if (Client is null)
                return -1;

            var filter = Builders<BsonDocument>.Filter.Empty;

            var database = Client.GetDatabase(dbName);
            var collection = database.GetCollection<BsonDocument>(collectionName);

            if (!string.IsNullOrWhiteSpace(searchValue))
            {
                if (!string.IsNullOrWhiteSpace(selectedKey))
                {
                    filter = Builders<BsonDocument>.Filter.Regex(selectedKey, new BsonRegularExpression(searchValue, "i"));
                }
                else
                {
                    var fieldNames = collection.Find(new BsonDocument()).Limit(1).FirstOrDefault()?.Names.ToList();
                    var filters = new List<FilterDefinition<BsonDocument>>();

                    if (fieldNames != null)
                    {
                        foreach (var field in fieldNames)
                        {
                            filters.Add(Builders<BsonDocument>.Filter.Regex(field, new BsonRegularExpression(searchValue, "i")));
                        }
                    }

                    filter = Builders<BsonDocument>.Filter.Or(filters);
                }
            }

            return collection.CountDocuments(filter);
        }


        public List<BsonDocument> GetCollection(string dbName, string collectionName, int skip, int limit, string selectedKey, string searchValue)
        {
            if (Client is null)
                return new List<BsonDocument>();

            var filter = Builders<BsonDocument>.Filter.Empty;

            var database = Client.GetDatabase(dbName);
            var collection = database.GetCollection<BsonDocument>(collectionName);

            if (!string.IsNullOrWhiteSpace(searchValue))
            {
                if (!string.IsNullOrWhiteSpace(selectedKey))
                {
                    filter = Builders<BsonDocument>.Filter.Regex(selectedKey, new BsonRegularExpression(searchValue, "i"));
                }
                else
                {
                    var fieldNames = collection.Find(new BsonDocument()).Limit(1).FirstOrDefault()?.Names.ToList();
                    var filters = new List<FilterDefinition<BsonDocument>>();

                    if (fieldNames != null)
                    {
                        foreach (var field in fieldNames)
                        {
                            filters.Add(Builders<BsonDocument>.Filter.Regex(field, new BsonRegularExpression(searchValue, "i")));
                        }
                    }

                    filter = Builders<BsonDocument>.Filter.Or(filters);
                }
            }

            return collection.Find(filter).Skip(skip).Limit(limit).ToList().Select(doc => doc).ToList();
        }

        public int GetCollectionCount(string dbName, string collectionName, string selectedKey, string searchValue)
        {
            if (Client is null)
                return -1;

            var filter = Builders<BsonDocument>.Filter.Empty;
            if (!string.IsNullOrWhiteSpace(selectedKey) && !string.IsNullOrWhiteSpace(searchValue))
            {
                filter = Builders<BsonDocument>.Filter.Regex(selectedKey, new BsonRegularExpression(searchValue, "i"));
            }

            var database = Client.GetDatabase(dbName);
            var collection = database.GetCollection<BsonDocument>(collectionName);
            return (int)collection.CountDocuments(filter);
        }

        public async Task<bool> InsertDocumentAsync(string dbName, string collectionName, dynamic document)
        {
            if (Client is null)
                return false;

            try
            {
                var database = Client.GetDatabase(dbName);
                var collection = database.GetCollection<BsonDocument>(collectionName);
                var doc = BsonDocument.Parse(JObject.FromObject(document).ToString());

                await collection.InsertOneAsync(doc);
                LogManager _ = new LogManager(LogType.Info, "User: " + Username + " has inserted a document into DB: " + dbName);

                return true;
            }
            catch (Exception e)
            {
                LogManager _ = new LogManager(LogType.Error, "User: " + Username + " failed to insert document into DB: " + dbName + " " + e);
                return false;
            }
        }

        public async Task<bool> UpdateMongoDB(string dbName, string collectionName, Dictionary<string, object> differences, Dictionary<string, string> renameMap, string id)
        {
            if (Client == null)
                return false;

            try
            {
                var database = Client.GetDatabase(dbName);
                var collection = database.GetCollection<BsonDocument>(collectionName);

                var filter = Builders<BsonDocument>.Filter.Eq("_id", new ObjectId(id));
                var updateDef = Builders<BsonDocument>.Update;

                if (differences.ContainsKey("") && differences[""] is JObject replacementDoc)
                {
                    var replacementBson = BsonDocument.Parse(replacementDoc.ToString());
                    await collection.ReplaceOneAsync(filter, replacementBson);
                    return true;
                }

                foreach (var rename in renameMap)
                {
                    var renameDefinition = updateDef.Rename(rename.Key, rename.Value);
                    await collection.UpdateOneAsync(filter, renameDefinition);
                }

                foreach (var diff in differences)
                {
                    BsonValue bsonValue;
                    var value = diff.Value;

                    if (value is JObject jObject)
                    {
                        bsonValue = BsonDocument.Parse(jObject.ToString());
                    }
                    else if (value == null)
                    {
                        bsonValue = BsonNull.Value;
                    }
                    else
                    {
                        bsonValue = ConvertToBsonValue(value);
                    }

                    var updateDefinition = updateDef.Set(diff.Key, bsonValue);
                    await collection.UpdateOneAsync(filter, updateDefinition);
                }

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }


        private BsonValue ConvertToBsonValue(object value)
        {
            switch (value)
            {
                case JObject jObject:
                    return BsonDocument.Parse(jObject.ToString());

                case JArray jArray:
                    return ConvertJArrayToBsonArray(jArray);

                case JValue jValue:
                    return ConvertJValueToBsonValue(jValue);

                case null:
                    return BsonNull.Value;

                default:
                    var bsonValue = BsonValue.Create(value);
                    return bsonValue;
            }
        }

        private BsonArray ConvertJArrayToBsonArray(JArray jArray)
        {
            var bsonArray = new BsonArray();
            foreach (var item in jArray)
            {
                bsonArray.Add(ConvertToBsonValue(item));
            }
            return bsonArray;
        }

        private BsonValue ConvertJValueToBsonValue(JValue jValue)
        {
            return BsonValue.Create(jValue.Value);
        }

        public bool DeleteAllDatabases()
        {
            if (Client is null)
                return false;

            bool overallResult = true;
            try
            {
                var databaseNames = Client.ListDatabaseNames().ToList();
                foreach (var dbName in databaseNames)
                {
                    if(dbName == "admin" || dbName == "config" || dbName == "local")
                        continue;
                    
                    if (!DeleteDB(dbName))
                    {
                        overallResult = false;
                    }
                }
                LogManager _ = new(LogType.Info, "User: " + Username + " has deleted all databases.");
            }
            catch (Exception e)
            {
                LogManager _ = new(LogType.Error, "User: " + Username + " has failed to delete all databases. " + e);
                overallResult = false;
            }

            return overallResult;
        }

        /// <summary>
        /// Execute a MongoDB query
        /// </summary>
        /// <param name="dbName">Database name</param>
        /// <param name="collectionName">Collection name</param>
        /// <param name="query">MongoDB query string</param>
        /// <returns>String containing the result of the query</returns>
        public async Task<string> ExecuteMongoQuery(string dbName, string collectionName, string query)
        {
            if (Client is null)
                return "MongoDB client is not initialized.";

            string result;
            try
            {
                var db = Client.GetDatabase(dbName);
                var collection = db.GetCollection<BsonDocument>(collectionName);

                var filter = BsonDocument.Parse(query);
                var findResult = await collection.Find(filter).ToListAsync();

                result = Newtonsoft.Json.JsonConvert.SerializeObject(findResult);

                LogManager _ = new(LogType.Info, $"User: {Username} has executed the query: {query} on DB: {dbName}, Collection: {collectionName}");
            }
            catch (Exception e)
            {
                LogManager _ = new(LogType.Error, $"User: {Username} has failed to execute the query: {query} on DB: {dbName}, Collection: {collectionName} {e}");
                result = "Error: " + e.Message;
            }

            return result;
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
        public bool CheckIfDBExist(string? dbName)
        {
            if (Client is null || dbName is null)
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

        /// <summary>
        /// Get all statistics of a specific MongoDB database
        /// </summary>
        /// <param name="dbName">Database Name</param>
        /// <returns>A BsonDocument containing all the database statistics or null if failed</returns>
        public BsonDocument? GetDatabaseStatistics(string dbName)
        {
            if (Client is null)
                return null;

            try
            {
                var db = Client.GetDatabase(dbName);
                var command = new BsonDocument { { "dbStats", 1 }, { "scale", 1 } };
                var stats = db.RunCommand<BsonDocument>(command);
                return stats;
            }
            catch (Exception e)
            {
                LogManager _ = new(LogType.Error, "User: " + Username + " failed to fetch statistics for DB: " + dbName, e);
                return null;
            }
        }

        /// <summary>
        /// Get all statistics of a specific MongoDB collection
        /// </summary>
        /// <param name="dbName">Database Name</param>
        /// <param name="collectionName">Collection Name</param>
        /// <returns>A BsonDocument containing all the collection statistics or null if failed</returns>
        public BsonDocument? GetCollectionStatistics(string dbName, string collectionName)
        {
            if (Client is null)
                return null;

            try
            {
                var db = Client.GetDatabase(dbName);
                var collection = db.GetCollection<BsonDocument>(collectionName);
                var command = new BsonDocument { { "collStats", collectionName } };
                var stats = db.RunCommand<BsonDocument>(command);
                return stats;
            }
            catch (Exception e)
            {
                LogManager _ = new LogManager(LogType.Error, "User: " + Username + " failed to fetch statistics for collection: " + collectionName, e);
                return null;
            }
        }


        /// <summary>
        /// Get global statistics of the MongoDB instance
        /// </summary>
        /// <returns>A BsonDocument containing all the server statistics or null if failed</returns>
        public BsonDocument? GetGlobalStatistics()
        {
            if (Client is null)
                return null;

            try
            {
                var db = Client.GetDatabase("admin");
                var command = new BsonDocument { { "serverStatus", 1 } };
                var stats = db.RunCommand<BsonDocument>(command);
                return stats;
            }
            catch (Exception e)
            {
                LogManager _ = new(LogType.Error, "User: " + Username + " failed to fetch global statistics.", e);
                return null;
            }
        }

        public async Task StreamCollectionExport(StreamWriter writer, string dbName, string collectionName, Guid guid)
        {
            if (Client is null)
                return;

            try
            {
                var db = Client.GetDatabase(dbName);
                var collectionData = db.GetCollection<BsonDocument>(collectionName);
                var filter = new BsonDocument();

                await writer.WriteAsync($"{{\"{dbName}\":{{\"{collectionName}\": [");

                var totalDocuments = await collectionData.CountDocumentsAsync(filter);
                var processedDocuments = 0;

                using (var cursor = collectionData.Find(filter).ToCursor())
                {
                    var jsonWriterSettings = new JsonWriterSettings { OutputMode = JsonOutputMode.CanonicalExtendedJson };
                    bool isFirstDocument = true;
                    var progress = 0;

                    while (await cursor.MoveNextAsync())
                    {
                        foreach (var document in cursor.Current)
                        {
                            progress = (int)((double)processedDocuments / totalDocuments * 100);
                            await _hubContext!.Clients.All.SendAsync("ReceiveProgressCollection", totalDocuments, processedDocuments, progress, guid.ToString(), "download");
                            if (!isFirstDocument)
                            {
                                await writer.WriteAsync(",");
                            }

                            var documentJson = document.ToJson(jsonWriterSettings);
                            await writer.WriteAsync(documentJson);

                            isFirstDocument = false;
                            processedDocuments++;
                            progress = (int)((double)processedDocuments / totalDocuments * 100);
                            await _hubContext!.Clients.All.SendAsync("ReceiveProgressCollection", totalDocuments, processedDocuments, progress, guid.ToString(), "download");
                        }
                    }
                }

                await writer.WriteAsync("]}}");
            }
            catch (Exception e)
            {
                LogManager _ = new LogManager(LogType.Error, $"User: {Username} has failed to load the Collection: {collectionName} from DB: {dbName}", e);
            }
        }

        /// <summary>
        /// Get all collections from a Database, the return has every data from every collection in the DB
        /// </summary>
        /// <param name="dbName">Database Name</param>
        /// <returns>Returns JObject?</returns>
        public async Task StreamAllCollectionExport(StreamWriter writer, string dbName, Guid guid)
        {
            if (Client is null)

                return;

            try
            {
                var progress = 0;
                var db = Client.GetDatabase(dbName);
                var collections = db.ListCollections().ToList();

                int totalCollections = collections.Count;
                int processedCollections = 0;

                await writer.WriteAsync("{\"" + dbName + "\":{");

                var jsonWriterSettings = new JsonWriterSettings { OutputMode = JsonOutputMode.CanonicalExtendedJson };
                bool isFirstCollection = true;

                foreach (var collection in collections)
                {
                    progress = (int)((double)processedCollections / totalCollections * 100);
                    await _hubContext!.Clients.All.SendAsync("ReceiveProgressDatabase", totalCollections, processedCollections, progress, guid.ToString(), "download");
                    if (!isFirstCollection)
                    {
                        await writer.WriteAsync(",");
                    }

                    var collectionName = collection["name"].AsString;
                    await writer.WriteAsync($"\"{collectionName}\": [");

                    var collectionData = db.GetCollection<BsonDocument>(collectionName);
                    var filter = new BsonDocument();

                    using (var cursor = collectionData.Find(filter).ToCursor())
                    {
                        bool isFirstDocument = true;
                        while (await cursor.MoveNextAsync())
                        {
                            foreach (var document in cursor.Current)
                            {
                                if (!isFirstDocument)
                                {
                                    await writer.WriteAsync(",");
                                }

                                var documentJson = document.ToJson(jsonWriterSettings);
                                await writer.WriteAsync(documentJson);

                                isFirstDocument = false;
                            }
                        }
                    }

                    await writer.WriteAsync("]");
                    isFirstCollection = false;
                    processedCollections++;
                }
                progress = (int)((double)processedCollections / totalCollections * 100);
                await _hubContext!.Clients.All.SendAsync("ReceiveProgressDatabase", totalCollections, processedCollections, progress, guid.ToString(), "download");
                await writer.WriteAsync("}}");
            }
            catch (Exception e)
            {
                LogManager _ = new LogManager(LogType.Error, "User: " + Username + " has failed to load the All Collections from DB: " + dbName, e);
            }
        }

        public async Task<bool> UploadJSONAsync(string dbName, string collectionName, JToken json, bool adaptOid)
        {
            if (Client is null)
                return false;

            try
            {
                var db = Client.GetDatabase(dbName);
                var collection = db.GetCollection<BsonDocument>(collectionName);
                List<BsonDocument> batch = new List<BsonDocument>();

                if (json is JArray jsonArray)
                {
                    foreach (JObject jsonObject in jsonArray)
                    {
                        if (!adaptOid && jsonObject.ContainsKey("_id"))
                        {
                            jsonObject.Remove("_id");
                        }

                        var document = BsonDocument.Parse(jsonObject.ToString());
                        batch.Add(document);

                        if (batch.Count >= BatchCount)
                        {
                            await collection.InsertManyAsync(batch);
                            batch.Clear();
                        }
                    }

                    if (batch.Count > 0)
                    {
                        await collection.InsertManyAsync(batch);
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
                    await collection.InsertOneAsync(document);
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task GenerateRandomData(string dbName, int collectionsCount, int totalDocuments, Guid guid)
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
                List<BsonDocument> batch = new List<BsonDocument>();

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

                    batch.Add(randomData);

                    if (batch.Count >= BatchCount)
                    {
                        collection.InsertManyAsync(batch).Wait();
                        batch.Clear();
                    }

                    var progress = (int)((double)i / collectionsCount * 100);
                    await _hubContext!.Clients.All.SendAsync("ReceiveProgressDatabase", collectionsCount, i, progress, guid.ToString(), "generate");
                }

                if (batch.Count > 0)
                {
                    collection.InsertManyAsync(batch).Wait();
                }
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
