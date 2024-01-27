
namespace BotApi.Database
{
    public interface IDB
    {
        Client? GetClient(long chatId);
        void AddClient(long chatId);
        Client? RemoveClient(long chatId);

        Subscription? GetSubscription(long chatId, SubscriptionType type);
        bool HaveSubscription(long chatId, SubscriptionType type);
        void AddSubscription(long chatId, SubscriptionType subscriptionType);
        Subscription? RemoveSubscription(long chatId, SubscriptionType subscriptionType);

        IEnumerable<string> GetClientsWithSubscription(SubscriptionType subscriptionType);
    }
}
