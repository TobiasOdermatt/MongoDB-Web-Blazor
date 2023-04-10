using MongoDB_Web.Data.DB;

namespace MongoDB_Web.Data.Helpers
{
    public class ConfigManager
    {
        public static IConfiguration? Config;

        public ConfigManager() { }

        public ConfigManager(IConfiguration config)
        {
            //Load the DBConnector with the config.json variables
            DBConnector _LoadDBConnector = new(config);
            Config = config;
        }

        public String? ReadKey(string key)
        {
            return Config[key];
        }
    }
}
