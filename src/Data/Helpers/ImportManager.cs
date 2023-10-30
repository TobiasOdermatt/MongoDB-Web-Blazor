using Microsoft.AspNetCore.SignalR;
using MongoDB_Web.Controllers;
using MongoDB_Web.Data.Hubs;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace MongoDB_Web.Data.Helpers
{
    public class ImportManager
    {
        public readonly IHubContext<ProgressHub>? _hubContext;

        public ImportManager(IHubContext<ProgressHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task<(string, List<string>)> ProcessDBImportAsync(string filePath, Guid guid)
        {
            List<string> collectionsNames = new();
            string dbName = "";
            bool isRoot = true;

            long totalBytes = new FileInfo(filePath).Length;
            long bytesRead = 0;
            long lastReportedBytesRead = 0;

            using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            using (var streamReader = new StreamReader(fileStream))
            using (var jsonReader = new JsonTextReader(streamReader))
            {
                var jsonSerializer = new JsonSerializer();
                while (await jsonReader.ReadAsync())
                {
                    bytesRead = fileStream.Position;
                    double progress = (double)bytesRead / totalBytes * 100;
                    if (Math.Abs(bytesRead - lastReportedBytesRead) >= (totalBytes * 0.01))
                    {
                        double totalMB = totalBytes / (1024.0 * 1024.0);
                        double bytesReadMB = bytesRead / (1024.0 * 1024.0);

                        await _hubContext!.Clients.All.SendAsync("ReceiveProgress", totalMB, bytesReadMB, progress, guid.ToString(), "MB");

                        lastReportedBytesRead = bytesRead;
                    }

                    if (jsonReader.TokenType == JsonToken.PropertyName)
                    {
                        var propertyName = jsonReader.Value?.ToString();
                        if (!string.IsNullOrEmpty(propertyName))
                        {
                            if (isRoot)
                            {
                                dbName = propertyName;
                                isRoot = false;
                            }
                            if (await jsonReader.ReadAsync() && jsonReader.TokenType == JsonToken.StartArray)
                            {
                                collectionsNames.Add(propertyName);
                            }
                        }
                    }
                }
            }

            return (dbName, collectionsNames);
        }

        public async Task<bool> ImportCollectionsAsync(string dbName, List<string> checkedCollectionNames, Dictionary<string, string> collectionNameChanges, bool adoptOid, DBController dBController, string filePath, Guid guid)
        {
            if (string.IsNullOrEmpty(filePath))
                return false;

            long totalBytes = new FileInfo(filePath).Length;
            long bytesRead = 0;
            long lastReportedBytesRead = 0;

            using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            using (var streamReader = new StreamReader(fileStream))
            using (var jsonReader = new JsonTextReader(streamReader))
            {
                var jsonSerializer = new JsonSerializer();
                while (await jsonReader.ReadAsync())
                {
                    bytesRead = fileStream.Position;
                    double progress = (double)bytesRead / totalBytes * 100;
                    double bytes = ((double)bytesRead) / (1024.0 * 1024.0);
                    double total = ((double)totalBytes) / (1024.0 * 1024.0);
                    if (Math.Abs(bytesRead - lastReportedBytesRead) >= (totalBytes * 0.01))
                    {

                        await _hubContext!.Clients.All.SendAsync("ReceiveProgress", total, bytes, progress, guid.ToString(), "MB");
                        lastReportedBytesRead = bytesRead;

                    }

                    if (jsonReader.TokenType == JsonToken.PropertyName)
                    {
                        var propertyName = jsonReader.Value?.ToString();
                        if (!string.IsNullOrEmpty(propertyName))
                        {
                            if (await jsonReader.ReadAsync() && jsonReader.TokenType == JsonToken.StartArray)
                            {
                                if (checkedCollectionNames.Contains(propertyName))
                                {
                                    string collectionNameToImport = collectionNameChanges.ContainsKey(propertyName) ? collectionNameChanges[propertyName] : propertyName;

                                    while (await jsonReader.ReadAsync() && jsonReader.TokenType != JsonToken.EndArray)
                                    {
                                        if (jsonReader.TokenType == JsonToken.StartObject)
                                        {
                                            JObject document = await JObject.LoadAsync(jsonReader);
                                            await dBController.UploadJSONAsync(dbName, collectionNameToImport, document, adoptOid);
                                        }
                                    }
                                }

                            }
                        }
                    }
                }
            }

            return true;
        }

    }
}