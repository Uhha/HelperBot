using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types;
using QBittorrent.Client;
using static BotApi.Services.QBitService;

namespace BotApi.Interfaces
{
    public interface IQBitService
    {
        Task<int> StartSearchAsync(string query);

        Task<SearchResults> GetSearchResultsAsync(int searchId);

        Task<IReadOnlyList<SearchPlugin>> GetSearchPluginsAsync();

        Task EnablePluginAsync(string pluginName);

        Task DisablePluginAsync(string pluginName);

        Task AddTorrentAsync(string torrentFileUrl, string downloadFolder);
    }
}
