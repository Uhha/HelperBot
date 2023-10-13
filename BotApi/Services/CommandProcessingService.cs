using BotApi.Interfaces;
using Telegram.Bot.Types;

namespace BotApi.Services
{
    public class CommandProcessingService : ICommandProcessingService
    {
        public async Task ProcessCommandAsync(CommandType commandType, bool isCallBack, Update update, ITelegramBotService bot)
        {
            switch (commandType)
            {
                case CommandType.ComicSubscribe:
                    break;
                case CommandType.FincanceSubscribe:
                    break;
                case CommandType.Coins:
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
