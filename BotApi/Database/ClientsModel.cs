using Newtonsoft.Json;
using Telegram.Bot.Types;

namespace BotApi.Database
{
    public class ClientsModel
    {
        [JsonProperty("clients")]
        public IList<Client> Clients { get; set; }

        public ClientsModel()
        {
            Clients = new List<Client>();
        }
    }

    public class Client
    {
        [JsonProperty("chatid")]
        public string ChatId { get; set; }

        [JsonProperty("subscriptions")]
        public IList<Subscription> Subscriptions { get; set; }

        [JsonProperty("securities")]
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
        [JsonProperty("type")]
        public SubscriptionType Type { get; set; }

        [JsonProperty("last_posted_key")]
        public string? LastPostedKey { get; set; }

        public Subscription(SubscriptionType type)
        {
            Type = type;
        }
    }

    public enum SubscriptionType
    {
        Oglaf,
        XKCD,
        CoinCapMarket,
    }

}
