using BotApi.Commands;
using BotApi.Interfaces;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using static System.Net.Mime.MediaTypeNames;

namespace BotApi.Services
{
    public class TelegramBotService : ITelegramBotService
    {
        private readonly TelegramBotClient _botClient;
        private readonly ILogger<TelegramBotService> _logger;

        public TelegramBotService(string botToken, ILogger<TelegramBotService> logger)
        {
            _botClient = new TelegramBotClient(botToken);
            _logger = logger;
        }

        public async Task SendTextMessageAsync(long chatId, string message, ParseMode? parseMode = null)
        {
            if (string.IsNullOrEmpty(message))
            {
                _logger.LogWarning("Trying to send an empty message");
                return;
            }
            await _botClient.SendTextMessageAsync(chatId, message, parseMode: parseMode);
        }

        public async Task SendChatActionAsync(long? chatId, ChatAction chatAction)
        {
            try
            {
                await _botClient.SendChatActionAsync(chatId, chatAction);
            }
            catch (Exception)
            {
                
            }
        }

        public async Task SetWebhookAsync(string webHookUrl)
        {
            await _botClient.SetWebhookAsync(webHookUrl);
        }

        public async Task<Message> ReplyAsync(Update update, string message) 
        {
            message = TruncateLongMessage(message);

            if (update.Message != null)
                return await _botClient.SendTextMessageAsync(update.Message.From.Id, message);
            else if (update.CallbackQuery != null)
                return await _botClient.SendTextMessageAsync(update.CallbackQuery.From.Id, message);

            return null;
        }

        public async Task<Message> ReplyAsync(long chatId, string message)
        {
            message = TruncateLongMessage(message);
            return await _botClient.SendTextMessageAsync(chatId, message);
        }

        public async Task SendTextMessageWithButtonsAsync(Update update, string message, IReplyMarkup replyMarkup)
        {
            if (update.Message != null)
            {
                await _botClient.SendTextMessageAsync(update.Message.From.Id, message, replyMarkup: replyMarkup);
            }
            else if (update.CallbackQuery != null)
            {
                await _botClient.SendTextMessageAsync(update.CallbackQuery.From.Id, message, replyMarkup: replyMarkup);
            }
        }

        private string TruncateLongMessage(string input)
        {
            Encoding encoding = Encoding.UTF8;
            int maxBytes = 4096; //Max length of a telegram message
            byte[] bytes = encoding.GetBytes(input);

            if (bytes.Length <= maxBytes)
            {
                // The input is within the desired limit, no need to truncate
                return input;
            }

            byte[] truncatedBytes = new byte[maxBytes];
            Array.Copy(bytes, truncatedBytes, maxBytes);

            int byteCount = encoding.GetCharCount(truncatedBytes); // Calculate char count

            return input.Substring(0, byteCount);
        }

		public async Task SendFileAsync(Update update, string filePath, string? filename)
		{
            try
            {
                using (var fileStream = new FileStream(filePath, FileMode.Open))
                {
                    var fileInput = new InputFileStream(fileStream, filename ?? filePath);
                    await _botClient.SendDocumentAsync(update.Message.From.Id, fileInput);
                }
            }
            catch (Exception e)
            {
                await this.ReplyAsync(update, e.Message);
            }
		}

        public async Task SendPhotoAsync(long chatId, InputFileUrl url)
        {
            try
            {
                await _botClient.SendPhotoAsync(chatId, url);
            }
            catch (Exception e)
            {
                await this.ReplyAsync(chatId, e.Message);
            }
        }

        public async Task EditMessageAsync(ChatId chatId, int messageId, string message, ParseMode parseMode)
        {
            await _botClient.EditMessageTextAsync(chatId, messageId,message, parseMode);
        }
    }
}
