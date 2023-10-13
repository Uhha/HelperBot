using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types;
using static BotApi.Services.WebhookService;
using Telegram.Bot;

namespace BotApi.Interfaces
{
    public interface ICommandProcessingService
    {
        Task ProcessCommandAsync(CommandType commandType, bool isCallBack, Update update, ITelegramBotService bot);
    }
}
