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
        public static void SetConfig(IConfiguration configuration) => _config = configuration;

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
            get { return _config["SQLSERVER_CONNECTION_STRING"]; }
        }



    }
}
