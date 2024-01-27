using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types;
using QBittorrent.Client;
using static BotApi.Services.QBitService;
using Microsoft.Extensions.Options;

namespace BotApi.Interfaces
{
    public interface IGetCoinsService
    {
        Task<(string, bool)> GetPricesAsync(int currenciesNumber);
    }
}
