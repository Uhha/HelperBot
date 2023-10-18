using Telegram.Bot.Types;
using static BotApi.Commands.RegisterCommands;

namespace BotApi.Interfaces
{
    public interface IWebhookService
    {
        (CommandType command, bool isCallback) GetCommandType(Update update);
    }

    
}
