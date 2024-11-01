using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types;
using QBittorrent.Client;
using static BotApi.Services.QBitService;
using Microsoft.Extensions.Options;

namespace BotApi.Interfaces
{
    public interface ISecuritiesService
    {
        Task<IList<string>> GetPricesAsync(long chatId);
        bool AddSecurity(long chatId, string symbol);
        bool RemoveSecurity(long chatId, string symbol);
        IList<string> ListSecurities(long chatId);
    }
}
