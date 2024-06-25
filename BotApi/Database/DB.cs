using System.Runtime.CompilerServices;
using System.Text.Json;

namespace BotApi.Database
{
    public class DB : IDB
    {
        private ILogger<DB> _logger;
        private const string JSON_PATH = "Database/clients.json";
        private ClientsModel _clients;
        //private Timer _saveTimer;
        //private bool _isModified = false;
        private readonly object _lockObject = new object();

        public DB(ILogger<DB> logger)
        {
            _logger = logger;

            try
            {
                _clients = LoadClientsModel();
                //_saveTimer = new Timer(SaveClientsModel, null, TimeSpan.Zero, TimeSpan.FromMinutes(30));
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                throw;
            }
        }

        private ClientsModel LoadClientsModel()
        {
            if (File.Exists(JSON_PATH))
            {
                string json = File.ReadAllText(JSON_PATH);
                return JsonSerializer.Deserialize<ClientsModel>(json) ?? new ClientsModel();
            }
            else
            {
                return new ClientsModel();
            }
        }

        public void SaveClientsModel()
        {
            try
            {
                lock (_lockObject)
                {
                    string json = JsonSerializer.Serialize(_clients, new JsonSerializerOptions { WriteIndented = true });
                    File.WriteAllText(JSON_PATH, json);
                    _logger.LogInformation($"ClientsModel updated to {JSON_PATH}");
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
            }
        }

        public Client? GetClient(long chatId)
        {
            return _clients.Clients.FirstOrDefault(o => o.ChatId == chatId.ToString());
        }

        public void AddClient(long chatId)
        {
            lock (_lockObject)
            {
                if (!HaveClient(chatId))
                {
                    _clients.Clients.Add(new Client(chatId.ToString()));
                    _logger.LogInformation($"New Client Added {chatId}");
                    SaveClientsModel();
                }
            }
        }

        public Client? RemoveClient(long chatId)
        {
            try
            {
                Client? client = null;
                if (HaveClient(chatId))
                {
                    lock (_lockObject)
                    {
                        client = GetClient(chatId);
                        _clients.Clients.Remove(client);
                        _logger.LogInformation($"Removed client {client}");
                        SaveClientsModel();
                    }
                }
                return client;
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                throw;
            }
        }

        private bool HaveClient(long chatId)
        {
            return GetClient(chatId) != null;
        }

        public Subscription? GetSubscription(long chatId, SubscriptionType subscriptionType)
        {
            return GetClient(chatId)?.Subscriptions.FirstOrDefault(o => o.Type == subscriptionType);
        }

        public void AddSubscription(long chatId, SubscriptionType subscriptionType)
        {
            try
            {
                lock (_lockObject)
                {
                    var client = GetClient(chatId);
                    if (client == null)
                    {
                        AddClient(chatId);
                    }

                    client?.Subscriptions.Add(new Subscription(subscriptionType));
                    _logger.LogInformation($"Subscription {subscriptionType} added for user {chatId}");
                    SaveClientsModel();
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
            }
        }

        public bool HaveSubscription(long chatId, SubscriptionType subscriptionType)
        {
            return GetSubscription(chatId, subscriptionType) != null;
        }

        public Subscription? RemoveSubscription(long chatId, SubscriptionType subscriptionType)
        {
            try
            {
                var subscription = GetSubscription(chatId, subscriptionType);

                if (subscription != null)
                {
                    GetClient(chatId)?.Subscriptions.Remove(subscription);
                    _logger.LogInformation($"Removed subscription: {subscription}");
                    SaveClientsModel();
                }
                return subscription;
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                return null;
            }
        }

        public IEnumerable<string> GetClientsWithSubscription(SubscriptionType subscriptionType)
        {
            return _clients.Clients.Where(o => o.Subscriptions.Any(s => s.Type == subscriptionType)).Select(c => c.ChatId);
        }

        public IList<string> GetSecurities(long chatId)
        {
            return GetClient(chatId)?.Securities ?? new List<string>();
        }

        public bool AddSecurity(long chatId, string symbol)
        {
            try
            {
                var client = GetClient(chatId);
                if (client.Securities.Contains(symbol))
                    return true;

                client.Securities.Add(symbol);
                SaveClientsModel();
                return true;
            }
            catch (Exception e)
            {
                _logger?.LogError(e, e.Message);
                return false;
            }
        }

        public bool RemoveSecurity(long chatId, string symbol)
        {
            try
            {
                var client = GetClient(chatId);
                if (!client.Securities.Contains(symbol))
                    return true;

                client.Securities.Remove(symbol);
                SaveClientsModel();
                return true;
            }
            catch (Exception e)
            {
                _logger?.LogError(e, e.Message);
                return false;
            }
        }
    }
}
