﻿using BotApi.Commands;
using BotApi.Interfaces;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace BotApi.Services
{
    public class TelegramBotService : ITelegramBotService
    {
        private readonly TelegramBotClient _botClient;

        public TelegramBotService(string botToken)
        {
            _botClient = new TelegramBotClient(botToken);
        }

        public async Task SendTextMessageAsync(long chatId, string message, ParseMode? parseMode = null)
        {
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

        public async Task ReplyAsync(Update update, string message) 
        {
            message = TruncateLongMessage(message);

            if (update.Message != null)
                await _botClient.SendTextMessageAsync(update.Message.From.Id, message);
            else if (update.CallbackQuery != null)
                await _botClient.SendTextMessageAsync(update.CallbackQuery.From.Id, message);
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
    }
}
