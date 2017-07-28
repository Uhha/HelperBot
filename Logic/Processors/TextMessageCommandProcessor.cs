using DatabaseInteractions;
using Logic.Grabbers;
using System;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
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

            //if (update.Message.Text.Equals("/unsubs"))
            //{
            //    var inlineKeyboardMarkup = new InlineKeyboardMarkup
            //    {
            //        InlineKeyboard = new[]
            //        {
            //        new [] {  InlineKeyboardButton.WithCallbackData ("Oglaf", "-subOglaf"),
            //                 new InlineKeyboardButton ("xkcd", "-subXkcd")}
            //    }
            //    };
            //    await bot.SendTextMessageAsync(update.Message.Chat.Id, "Unsubscribe from:", replyMarkup: inlineKeyboardMarkup);
            //}

            if (update.Message.Text.Equals("/vocab"))
            {
                var inlineKeyboardMarkup = new InlineKeyboardMarkup
                {
                    InlineKeyboard = new[]
                    {
                        new [] {  InlineKeyboardButton.WithCallbackData (
                                    "Next Word", "vocabNW"),
                                InlineKeyboardButton.WithCallbackData (
                                    "Definition", "vocabDefinition=" + VocabCallbackData.Word.WordText )
                        }
                    }
                };

                try
                {
                    await bot.SendTextMessageAsync(update.Message.Chat.Id, VocabCallbackData.Message, 
                        replyMarkup: inlineKeyboardMarkup, parseMode: ParseMode.Html);
                    VocabCallbackData.PrepareNextWord();
                }
                catch (Exception e)
                {
                    //await bot.SendTextMessageAsync(update.Message.Chat.Id, e.Message);
                    //throw;
                }
            }

            if (update.Message.Text.Equals("/finance"))
            {
                var inlineKeyboardMarkup = new InlineKeyboardMarkup
                {
                    InlineKeyboard = new[]
                    {
                        new [] {  InlineKeyboardButton.WithCallbackData ("CoinCapMarket", "subCoinCM") }
                    }
                };
                await bot.SendTextMessageAsync(update.Message.Chat.Id, "Choose what to subscribe to:", replyMarkup: inlineKeyboardMarkup);
            }

            if (update.Message.Text.StartsWith("/coins"))
            {
                var number = update.Message.Text.Substring(update.Message.Text.IndexOf(' '));
                int.TryParse(number, out int currenciesNumber);
                var result = await CoinGrabber.GetPricesAsync(currenciesNumber);
                if (string.IsNullOrEmpty(result.Item1)) return;
                await bot.SendTextMessageAsync(update.Message.Chat.Id, result.Item1, parseMode: ParseMode.Html);
            }
        }
    }
}