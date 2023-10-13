using BotApi.Commands;
using BotApi.Interfaces;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace BotApi.Services
{
    public class TelegramBotService : ITelegramBotService
    {
        private readonly TelegramBotClient _botClient;
        private readonly CommandInvoker _commandInvoker;


        public TelegramBotService(string botToken)
        {
            _botClient = new TelegramBotClient(botToken);
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

        public async Task SetWebhookAsync(string webHookUrl)
        {
            await _botClient.SetWebhookAsync(webHookUrl);
        }
    }
}
