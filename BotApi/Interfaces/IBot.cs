using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types;

namespace BotApi.Interfaces
{
    public interface IBot
    {
        Task SendTextMessageAsync(long chatId, string message, ParseMode? parseMode = null);
        Task SendChatActionAsync(ChatId chatId, ChatAction chatAction);
    }
}
