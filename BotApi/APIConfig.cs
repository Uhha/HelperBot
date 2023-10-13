using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using System;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;

namespace BotApi
{
    public class APIConfig
    {
        public string? BotApiKey {  get; set; }

        public string? CoinMarketCapAPIKey { get; set; }

        public string? WebHookUrl { get; set; }

        public string? SQLSERVER_CONNECTION_STRING { get; set; }

        public string? QBUrl { get; set; }
    }
}
