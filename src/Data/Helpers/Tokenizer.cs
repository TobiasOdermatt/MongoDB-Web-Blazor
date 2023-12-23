using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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

            var oldJObject = JObject.Parse(preprocessedOldDocument);
            var newJObject = JObject.Parse(preprocessedDocument);

            if (!JToken.DeepEquals(oldJObject, newJObject))
            {
                return new Dictionary<string, object> { { "", newJObject } };
            }

            var differences = FindDifferences(oldJObject, newJObject);

            return differences.ToDictionary(kvp => kvp.Key, kvp => (object)kvp.Value);
        }


        private Dictionary<string, JToken> FindDifferences(JToken oldToken, JToken newToken)
        {
            var differences = new Dictionary<string, JToken>();

            if (oldToken.Type != newToken.Type)
            {
                differences[""] = newToken;
            }
            else if (oldToken.Type == JTokenType.Object)
            {
                var oldObject = (JObject)oldToken;
                var newObject = (JObject)newToken;

                foreach (var property in newObject.Properties())
                {
                    var oldProperty = oldObject.Property(property.Name);

                    if (oldProperty == null)
                    {
                        differences[property.Name] = property.Value;
                    }
                    else
                    {
                        var nestedDifferences = FindDifferences(oldProperty.Value, property.Value);
                        foreach (var kvp in nestedDifferences)
                        {
                            differences[property.Name + (kvp.Key == "" ? "" : "." + kvp.Key)] = kvp.Value;
                        }
                    }
                }
            }
            else if (!JToken.DeepEquals(oldToken, newToken))
            {
                differences[""] = newToken;
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
