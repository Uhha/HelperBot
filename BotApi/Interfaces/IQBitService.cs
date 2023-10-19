using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types;
using QBittorrent.Client;
using static BotApi.Services.QBitService;
using Microsoft.Extensions.Options;

namespace BotApi.Interfaces
{
    public interface IQBitService
    {
        Dictionary<string, long> ActiveTorrents { get; }

        Task<int> StartSearchAsync(string query);

        Task<SearchResults> GetSearchResultsAsync(int searchId);

        Task<IReadOnlyList<SearchPlugin>> GetSearchPluginsAsync();

        Task EnablePluginAsync(string pluginName);

        Task DisablePluginAsync(string pluginName);

        Task AddTorrentAsync(string torrentFileUrl, string downloadFolder, long user);

        Task Auth();

        Task<IReadOnlyList<TorrentInfo>> GetTorrentListAsync();

        Task DeleteTorrent(string torrentHash);
    }
}
