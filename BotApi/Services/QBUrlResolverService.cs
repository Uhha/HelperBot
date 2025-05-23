using BotApi.Interfaces;

namespace BotApi.Services
{
    public class QBUrlResolverService : IQBUrlResolverService
    {
        private static Dictionary<string, Uri> _urls;

        public QBUrlResolverService()
        {
            _urls = new Dictionary<string, Uri>();
        }

        public void Clear()
        {
            _urls.Clear();
        }

        public Uri GetUrl(string identifier)
        {
            if (_urls.ContainsKey(identifier))
            {
                return _urls[identifier];
            }
            return null;
        }

        public string SaveUrl(Uri url)
        {
            var urlKey = Guid.NewGuid().ToString();
            _urls.Add(urlKey, url);
            return urlKey;
        }

      
    }
}
