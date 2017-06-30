using System;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace Logic
{
    internal static class TextMessageCommandProcessor
    {
        internal static async Task ProcessAsync(TelegramBotClient bot, Update update)
        {
            if (update.Message.Text.Equals("/subs"))
            {
                var inlineKeyboardMarkup = new InlineKeyboardMarkup
                {
                    InlineKeyboard = new[]
                    {
                    new [] {  InlineKeyboardButton.WithCallbackData ("Oglaf", "subOglaf"),
                             new InlineKeyboardButton ("xkcd", "subXkdc")}
                }
                };
                await bot.SendTextMessageAsync(update.Message.Chat.Id, "Choose what to subscribe to:", replyMarkup: inlineKeyboardMarkup);
            }

            if (update.Message.Text.Equals("/unsubs"))
            {
                var inlineKeyboardMarkup = new InlineKeyboardMarkup
                {
                    InlineKeyboard = new[]
                    {
                    new [] {  InlineKeyboardButton.WithCallbackData ("Oglaf", "-subOglaf"),
                             new InlineKeyboardButton ("xkcd", "-subXkdc")}
                }
                };
                await bot.SendTextMessageAsync(update.Message.Chat.Id, "Unsubscribe from:", replyMarkup: inlineKeyboardMarkup);
            }
        }

    }
}