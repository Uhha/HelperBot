using BotApi.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QBittorrent.Client;

namespace BotApi.Services
{
    public class QBitService: IQBitService
    {
        private readonly QBittorrentClient _qBittorrentClient;
        private readonly IOptions<APIConfig> _apiConfig;
        private readonly ILogger<QBitService> _logger;

        public QBitService(ILogger<QBitService> logger, IOptions<APIConfig> apiConfig)
        {
            _logger = logger;
            _qBittorrentClient = new QBittorrentClient(new Uri(apiConfig.Value.QBUrl));
            _qBittorrentClient.LoginAsync(apiConfig.Value.QBLogin, apiConfig.Value.QBPassword);
            _apiConfig = apiConfig;
        }

        public async Task<int> StartSearchAsync(string query)
        {
            return await _qBittorrentClient.StartSearchAsync(query);
        }

        public async Task<SearchResults> GetSearchResultsAsync(int searchId)
        {
            return await _qBittorrentClient.GetSearchResultsAsync(searchId);
        }

        public async Task<IReadOnlyList<SearchPlugin>> GetSearchPluginsAsync()
        {
            return await _qBittorrentClient.GetSearchPluginsAsync();
        }

        public async Task EnablePluginAsync(string pluginName)
        {
            await _qBittorrentClient.EnableSearchPluginAsync(pluginName);
        }

        public async Task DisablePluginAsync(string pluginName)
        {
            await _qBittorrentClient.DisableSearchPluginAsync(pluginName);
        }

        public async Task AddTorrentAsync(string torrentFileUrl, string downloadFolder)
        {
            var tf = new AddTorrentUrlsRequest(new Uri(torrentFileUrl))
            {
                DownloadFolder = downloadFolder
            };

            SetCookie(tf, torrentFileUrl);
            
            await _qBittorrentClient.AddTorrentsAsync(tf);
        }

        private void SetCookie(AddTorrentUrlsRequest tf, string torrentFileUrl)
        {
            if (torrentFileUrl.Contains("rutracker"))
                tf.Cookie = _apiConfig.Value.RutrackerCookie;
        }
    }
}
