using Newtonsoft.Json;
using System.Text.Json.Serialization;
using Telegram.Bot.Types;

namespace BotApi.Database
{
    public class ClientsModel
    {
        [JsonPropertyName("clients")]
        public IList<Client> Clients { get; set; }

        public ClientsModel()
        {
            Clients = new List<Client>();
        }
    }

    public class Client
    {
        [JsonPropertyName("chatid")]
        public string ChatId { get; set; }

        [JsonPropertyName("subscriptions")]
        public IList<Subscription> Subscriptions { get; set; }

        [JsonPropertyName("securities")]
        public IList<string> Securities { get; set; }

        public Client(string chatId)
        {
            ChatId = chatId;
            Subscriptions = new List<Subscription>();
            Securities = new List<string>();
        }
    }

    public class Subscription
    {
        [JsonPropertyName("type")]
        public SubscriptionType Type { get; set; }

        [JsonPropertyName("last_posted_key")]
        public string? LastPostedKey { get; set; }

        public Subscription(SubscriptionType type)
        {
            Type = type;
        }
    }

    public enum SubscriptionType
    {
        Oglaf = 0,
        XKCD = 1,
        CoinCapMarket = 2,
        SecuritiesPrices = 3,
    }

}
