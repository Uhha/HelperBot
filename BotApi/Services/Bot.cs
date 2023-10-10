using BotApi.Interfaces;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace BotApi.Services
{
    public class Bot : IBot
    {
        private readonly TelegramBotClient _botClient;

        public Bot(string botToken, string webHookUrl)
        {
            _botClient = new TelegramBotClient(botToken);
            _botClient.SetWebhookAsync(webHookUrl);
            SendTextMessageAsync(182328439, "Local Bot started!");
        }
        public async Task SendTextMessageAsync(long chatId, string message, ParseMode? parseMode = null)
        {
            await _botClient.SendTextMessageAsync(chatId, message, parseMode: parseMode);
        }

        public async Task SendChatActionAsync(ChatId chatId, ChatAction chatAction)
        {
            await _botClient.SendChatActionAsync(chatId, chatAction);

        }
    }
}
