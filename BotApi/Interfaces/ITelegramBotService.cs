using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace BotApi.Interfaces
{
    public interface ITelegramBotService
    {
        Task SetWebhookAsync(string webHookUrl);
        Task SendTextMessageAsync(long chatId, string message, ParseMode? parseMode = null);
        Task SendChatActionAsync(long? chatId, ChatAction chatAction);
        Task ReplyAsync(Update update, string message);
        Task SendTextMessageWithButtonsAsync(Update update, string message, IReplyMarkup replyMarkup);
        Task SendFileAsync(Update update, string filePath);

	}
}
