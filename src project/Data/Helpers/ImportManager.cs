using MongoDB_Web.Data.DB;
using Newtonsoft.Json.Linq;
using System.Text;
using ThirdParty.Json.LitJson;

namespace MongoDB_Web.Data.Helpers
{
    public class ImportManager
    {
        static JObject? jsonData;

        public static (string, List<string>) ProcessDBImportAsync(byte[] fileContent)
        {
            string jsonString = Encoding.UTF8.GetString(fileContent);
            jsonData = JObject.Parse(jsonString);
            List<string> collectionsNames = new List<string>();
            string dbName = "";

            foreach (var db in jsonData)
            {
                dbName = db.Key;

                if (db.Value is JObject collectionData)
                {
                    foreach (var collection in collectionData)
                    {
                        collectionsNames.Add(collection.Key);
                    }
                }
            }

            return (dbName, collectionsNames);
        }

        public static bool ImportCollectionsAsync(string dbName, List<string> checkedCollectionNames, Dictionary<string, string> collectionNameChanges,bool adoptOid, DBController dBController)
        {
            if (jsonData is null)
                return false;

            foreach (var db in jsonData)
            {
                if (db.Value is JObject collectionData)
                {
                    foreach (var collection in collectionData)
                    {
                        if (checkedCollectionNames.Contains(collection.Key))
                        {
                            if (collection.Value != null)
                            {
                                string collectionNameToImport = collectionNameChanges.ContainsKey(collection.Key) ? collectionNameChanges[collection.Key] : collection.Key;
                                dBController.UploadJSON(dbName, collectionNameToImport, collection.Value, adoptOid);
                            }
                        }
                    }
                }
            }

            return true;
        }
    }
}