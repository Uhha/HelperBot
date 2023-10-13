using Telegram.Bot.Types;

namespace BotApi.Interfaces
{
    public interface IWebhookService
    {
        (CommandType command, bool isCallback) GetCommandType(Update update);
    }

    public enum CommandType
    {
        ComicSubscribe,
        FincanceSubscribe,
        Coins,
        WakeOnLan,
        Balance,
        BalanceAdd,
        BalanceRemove,
        BalanceDetails,
        Unknown,
    }
}
