using System.Collections.Specialized;
using System.Configuration;

namespace Logic
{
    public static class Config
    {
        private static readonly NameValueCollection Appsettings = ConfigurationManager.AppSettings;

        public static string BotApiKey
        {
            get { return Appsettings["BotApiKey"]; }
        }

        public static string CoinMarketCapAPIKey
        {
            get { return Appsettings["CoinMarketCapAPIKey"]; }
        }

        public static string WebHookUrl
        {
            get { return Appsettings["WebHookUrl"]; }
        }

        public static string Environment
        {
            get { return Appsettings["Environment"]; }
        }
        
    }
}
