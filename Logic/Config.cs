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

        public static string IsLocal
        {
            get { return Appsettings["IsLocal"]; }
        }
    }
}
