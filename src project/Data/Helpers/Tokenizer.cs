using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB_Web.Controllers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MongoDB_Web.Data.Helpers
{
    public class Tokenizer
    {
        private Dictionary<string?, object?> oldDoc;
        private Dictionary<string?, object?> newDoc;

        string PreprocessJson(string json)
        {
            return json.Replace("ObjectId(", "").Replace(")", "");
        }

        public Dictionary<string, object> FindDifferencesInDocument(string OldDocument, string Document)
        {
            string preprocessedOldDocument = PreprocessJson(OldDocument);
            string preprocessedDocument = PreprocessJson(Document);

            oldDoc = JsonConvert.DeserializeObject<Dictionary<string?, object?>>(preprocessedOldDocument) ?? new Dictionary<string?, object?>();
            newDoc = JsonConvert.DeserializeObject<Dictionary<string?, object?>>(preprocessedDocument) ?? new Dictionary<string?, object?>();

            var differences = new Dictionary<string, object>();

            foreach (var entry in newDoc)
            {
                if (!oldDoc.ContainsKey(entry.Key) || (entry.Value != null && !entry.Value.Equals(oldDoc[entry.Key])))
                {
                    differences[entry.Key] = entry.Value;
                }

            }

            return differences;
        }

        public Dictionary<string, string> FindRenameMap()
        {
            Dictionary<string, string> renameMap = new Dictionary<string, string>();

            foreach (var oldKey in oldDoc.Keys)
            {
                if (!newDoc.ContainsKey(oldKey))
                {
                    foreach (var newKey in newDoc.Keys)
                    {
                        if (oldDoc[oldKey].Equals(newDoc[newKey]))
                        {
                            renameMap[oldKey] = newKey;
                        }
                    }
                }
            }

            return renameMap;
        }


        public string? GetObjectId()
        {
            if (oldDoc.ContainsKey("_id") && oldDoc["_id"] != null)
            {
                return oldDoc["_id"].ToString();
            }
            return null;
        }

    }

}
