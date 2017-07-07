using DatabaseInteractions;
using System;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace Logic.Processors
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
                             new InlineKeyboardButton ("xkcd", "subXkcd")}
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
                             new InlineKeyboardButton ("xkcd", "-subXkcd")}
                }
                };
                await bot.SendTextMessageAsync(update.Message.Chat.Id, "Unsubscribe from:", replyMarkup: inlineKeyboardMarkup);
            }

            if (update.Message.Text.Equals("/vocab"))
            {
                var inlineKeyboardMarkup = new InlineKeyboardMarkup
                {
                    InlineKeyboard = new[]
                    {
                        new [] {  InlineKeyboardButton.WithCallbackData (
                                    "Next Word", "vocabNW"),
                                InlineKeyboardButton.WithCallbackData (
                                    "Definition", "vocabDefinition"  )
                        }
                        //new [] {  InlineKeyboardButton.WithCallbackData (
                        //            "Next Word", "vocabNW" )
                        //}
                    }
                };

                try
                {
                    await bot.SendTextMessageAsync(update.Message.Chat.Id, VocabCallbackData.Word, replyMarkup: inlineKeyboardMarkup);
                }
                catch (Exception e)
                {
                    await bot.SendTextMessageAsync(update.Message.Chat.Id, e.Message);
                    throw;
                }
            }
        }
    }
}