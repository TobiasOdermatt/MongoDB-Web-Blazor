using Newtonsoft.Json;

namespace MongoDB_Web.Data.Helpers
{
    public class Tokenizer
    {
        private Dictionary<string, object?> oldDoc;
        private Dictionary<string, object?> newDoc;


        public Tokenizer()
        {
            oldDoc = new Dictionary<string, object?>();
            newDoc = new Dictionary<string, object?>();
        }


        string preprocessJson(string json)
        {
            return json.Replace("ObjectId(", "").Replace(")", "");
        }

        public Dictionary<string, object> FindDifferencesInDocument(string OldDocument, string Document)
        {
            string preprocessedOldDocument = preprocessJson(OldDocument);
            string preprocessedDocument = preprocessJson(Document);

            oldDoc = JsonConvert.DeserializeObject<Dictionary<string, object?>>(preprocessedOldDocument) ?? new Dictionary<string, object?>();
            newDoc = JsonConvert.DeserializeObject<Dictionary<string, object?>>(preprocessedDocument) ?? new Dictionary<string, object?>();

            var differences = new Dictionary<string, object>();

            foreach (var entry in newDoc)
            {
                if (!oldDoc.ContainsKey(entry.Key) || (entry.Value != null && !entry.Value.Equals(oldDoc[entry.Key])))
                {
                    differences[entry.Key] = entry.Value == null ? "" : entry.Value;
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
                        if (object.Equals(oldDoc[oldKey], newDoc[newKey]))
                        {
                            renameMap[oldKey] = newKey;
                        }
                    }
                }
            }

            return renameMap;
        }
    }

}
