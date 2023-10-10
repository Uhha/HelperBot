using BotApi.Interfaces;
using BotApi.Modules;
using Telegram.Bot.Types;
using static BotApi.Services.WebhookService;

namespace BotApi.Services
{
    public class CommandProcessingService : ICommandProcessingService
    {
        public async Task ProcessCommandAsync(CommandType commandType, bool isCallBack, Update update, IBot bot)
        {
            switch (commandType)
            {
                case CommandType.ComicSubscribe:
                    break;
                case CommandType.FincanceSubscribe:
                    break;
                case CommandType.Coins:
                    await CoinModule.SendTopPricesAsync(bot, update);
                    break;
                case CommandType.WakeOnLan:
                    break;
                case CommandType.Balance:
                    break;
                case CommandType.BalanceAdd:
                    break;
                case CommandType.BalanceRemove:
                    break;
                case CommandType.BalanceDetails:
                    break;
                case CommandType.Unknown:
                    break;
                default:
                    break;
            }
        }
    }
}
