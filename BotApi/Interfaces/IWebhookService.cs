using Telegram.Bot.Types;
using static BotApi.Services.WebhookService;

namespace BotApi.Interfaces
{
    public interface IWebhookService
    {
        (CommandType command, bool isCallback) GetCommandType(Update update);
    }
}
