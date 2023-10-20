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
        public Dictionary<string, long> ActiveTorrents { get; }

        public QBitService(IOptions<APIConfig> apiConfig, ILogger<QBitService> logger)
        {
            _qBittorrentClient = new QBittorrentClient(new Uri(apiConfig.Value.QBUrl));
            _apiConfig = apiConfig;
            _logger = logger;
            ActiveTorrents = new Dictionary<string, long>();
        }

        public async Task Auth()
        {
            if (!string.IsNullOrEmpty(_apiConfig.Value.QBLogin) && !string.IsNullOrEmpty(_apiConfig.Value.QBPassword))
            {
                await _qBittorrentClient.LoginAsync(_apiConfig.Value.QBLogin, _apiConfig.Value.QBPassword);
            }
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

        public async Task AddTorrentAsync(string torrentFileUrl, string downloadFolder, long user)
        {
            var tf = new AddTorrentUrlsRequest(new Uri(torrentFileUrl))
            {
                DownloadFolder = downloadFolder
            };

            SetCookie(tf, torrentFileUrl);
            
            await _qBittorrentClient.AddTorrentsAsync(tf);

            await Task.Delay(TimeSpan.FromSeconds(15));
            await AddTorrentOwnership(tf, user);
        }

        private async Task AddTorrentOwnership(AddTorrentUrlsRequest tf, long user)
        {
            var torrents = await GetTorrentListAsync();
            var top = torrents.OrderByDescending(o => o.AddedOn).Take(1);
            ActiveTorrents.Add(top.First().Hash, user);
        }

        private void SetCookie(AddTorrentUrlsRequest tf, string torrentFileUrl)
        {
            if (torrentFileUrl.Contains("rutracker"))
                tf.Cookie = _apiConfig.Value.RutrackerCookie;
        }

        public async Task<IReadOnlyList<TorrentInfo>> GetTorrentListAsync()
        {
            return await _qBittorrentClient.GetTorrentListAsync();
        }

        public async Task DeleteTorrent(string torrentHash)
        {
            await _qBittorrentClient.DeleteAsync(torrentHash, deleteDownloadedData: false);
        }
    }
}
