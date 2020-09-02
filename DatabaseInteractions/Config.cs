using Microsoft.Extensions.Configuration;
using System.Collections.Specialized;
using System.Configuration;

namespace DatabaseInteractions
{
    public static class Config
    {

        private static IConfiguration _config;
        private static NameValueCollection _webConfig = ConfigurationManager.AppSettings;
        
        public static string BotApiKey
        {
            get { return _config["BotApiKey"] ?? _webConfig["BotApiKey"]; }
        }

        public static string CoinMarketCapAPIKey
        {
            get { return _config["CoinMarketCapAPIKey"] ?? _webConfig["CoinMarketCapAPIKey"]; }
        }

        public static string WebHookUrl
        {
            get { return _config["WebHookUrl"] ?? _webConfig["WebHookUrl"]; }
        }

        public static string Environment
        {
            get { return _config["Environment"] ?? _webConfig["Environment"]; }
        }

        public static string DBConnectionString
        {
            get { return _config["DBConnectionString"] ?? _webConfig["DBConnectionString"]; }
        }

        public static void SetConfig(IConfiguration icon) => _config = icon;

        public static string TEST
        {
            get { return "testval"; }
        }

    }
}
