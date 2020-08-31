using Microsoft.Extensions.Configuration;
using System.Collections.Specialized;
using System.Configuration;

namespace DatabaseInteractions
{
    public static class Config
    {
        public static IConfiguration _config;

        public static string BotApiKey
        {
            get { return _config["BotApiKey"]; }
        }

        public static string CoinMarketCapAPIKey
        {
            get { return _config["CoinMarketCapAPIKey"]; }
        }

        public static string WebHookUrl
        {
            get { return _config["WebHookUrl"]; }
        }

        public static string Environment
        {
            get { return _config["Environment"]; }
        }

        public static string DBConnectionString
        {
            get { return _config["DBConnectionString"]; }
        }


        
    }
}
