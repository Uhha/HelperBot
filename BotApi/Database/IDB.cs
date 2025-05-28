
namespace BotApi.Database
{
    public interface IDB
    {
        void SaveClientsModel();

        Client? GetClient(long chatId);
        void AddClient(long chatId);
        Client? RemoveClient(long chatId);

        Subscription? GetSubscription(long chatId, SubscriptionType type);
        bool HaveSubscription(long chatId, SubscriptionType type);
        void AddSubscription(long chatId, SubscriptionType subscriptionType);
        Subscription? RemoveSubscription(long chatId, SubscriptionType subscriptionType);

        IEnumerable<string> GetClientsWithSubscription(SubscriptionType subscriptionType);

        IList<string> GetSecurities(long chatId);
        bool AddSecurity(long chatId, string symbol);
        bool RemoveSecurity(long chatId, string symbol);

        bool AddBand(long chatId, string bandId, string bandName, string genre);
        string RemoveBand(long chatId, string bandId);
        IList<Band> GetBands(long chatId);
    }
}
