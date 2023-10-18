using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types;
using QBittorrent.Client;
using static BotApi.Services.QBitService;

namespace BotApi.Interfaces
{
    public interface IQBUrlResolverService
    {
        void Clear();
        string SaveUrl(Uri url);
        Uri GetUrl(string identifier);
    }
}
