using DatabaseInteractions;
using System;
using System.Linq;
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

            if (update.Message.Text.Equals("/vocab"))
            {
                string[] words;
                using (AlcoDBEntities db = new AlcoDBEntities())
                {
                    IQueryable<string> wrds = from p in db.Words
                                                   select p.Stem;
                    Random rnd = new Random();
                    words = wrds.ToArray().OrderBy(x => rnd.Next()).ToArray();
                }

                var inlineKeyboardMarkup = new InlineKeyboardMarkup
                {
                    InlineKeyboard = new[]
                    {
                        new [] {  InlineKeyboardButton.WithCallBackGame ("Next Word", new VocabCallbackData{ vocabCallbackType = VocabCallbackType.Word, words = words }),
                                InlineKeyboardButton.WithCallBackGame ("Definition", new VocabCallbackData{ vocabCallbackType = VocabCallbackType.Definition, words = words })},
                        
                    }
                };

                try
                {

                    await bot.SendTextMessageAsync(update.Message.Chat.Id, "xxx", replyMarkup: inlineKeyboardMarkup);
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