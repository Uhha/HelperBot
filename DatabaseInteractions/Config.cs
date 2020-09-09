using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;

namespace DatabaseInteractions
{
    public static class Config
    {

        private static IConfiguration _config;
        private static NameValueCollection _webConfig = ConfigurationManager.AppSettings;
        private static ConnectionStringSettingsCollection _connectionStrings = ConfigurationManager.ConnectionStrings;
        


        public static string BotApiKey
        {
            get { return string.IsNullOrEmpty(_config["BotApiKey"]) ? _webConfig["BotApiKey"] : _config["BotApiKey"]; }
        }

        public static string CoinMarketCapAPIKey
        {
            get { return string.IsNullOrEmpty(_config["CoinMarketCapAPIKey"]) ? _webConfig["CoinMarketCapAPIKey"] : _config["CoinMarketCapAPIKey"]; }
        }

        public static string WebHookUrl
        {
            get { return string.IsNullOrEmpty(_config["WebHookUrl"]) ? _webConfig["WebHookUrl"] : _config["WebHookUrl"]; }
        }

        public static string Environment
        {
            get { return string.IsNullOrEmpty(_config["Environment"]) ? _webConfig["Environment"] : _config["Environment"]; }
        }

        public static string DConStr
        {
            get { return _config.GetConnectionString("AlcoDB"); }
        }

        public static string DConStr2
        {
            get { return _config["DConStr"]; }
        }

        public static void SetConfig(IConfiguration icon) => _config = icon;

        public static string TEST
        {
            get { return "testval"; }
        }

    }
}
