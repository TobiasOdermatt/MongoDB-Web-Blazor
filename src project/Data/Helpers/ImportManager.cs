using MongoDB_Web.Data.DB;
using Newtonsoft.Json.Linq;
using System.Text;

namespace MongoDB_Web.Data.Helpers
{
    public class ImportManager
    {
        public static JObject? JsonData;

        public static (string, List<string>) ProcessDBImportAsync(byte[] fileContent)
        {
            string jsonString = Encoding.UTF8.GetString(fileContent);
            JsonData = JObject.Parse(jsonString);
            List<string> collectionsNames = new List<string>();
            string dbName = "";

            foreach (var db in JsonData)
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

        public static bool ImportCollectionsAsync(string dbName, List<string> checkedCollectionNames, DBController dBController)
        {
            if (JsonData is null)
                return false;

            foreach (var db in JsonData)
            {
                if (db.Value is JObject collectionData)
                {
                    foreach (var collection in collectionData)
                    {
                        if (checkedCollectionNames.Contains(collection.Key))
                            if (collection.Value != null)
                                dBController.UploadJSON(dbName, collection.Key, collection.Value);
                    }
                }
            }

            return true;
        }
    }
}