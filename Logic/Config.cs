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

        public static string TestBotApiKey
        {
            get { return Appsettings["TestBotApiKey"]; }
        }

        public static string WebHookUrl
        {
            get { return Appsettings["WebHookUrl"]; }
        }

        public static string Environment
        {
            get { return Appsettings["Environment"]; }
        }

        public static string OxfordUrl
        {
            get { return Appsettings["OxfordUrl"]; }
        }

        public static string OxfordAppId
        {
            get { return Appsettings["OxfordAppId"]; }
        }

        public static string OxfordAppKey
        {
            get { return Appsettings["OxfordAppKey"]; }
        }

        public static string OxfordLang
        {
            get { return Appsettings["OxfordLang"]; }
        }
    }
}
