using DatabaseInteractions;
using System;
using System.Linq;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Logic.Processors
{
    internal static class CallbackQueryCommandProcessor
    {
        internal static async void Process(TelegramBotClient bot, Update update)
        {
            if (update.CallbackQuery.Data.StartsWith("sub"))
            {
                Subscription subscriptionType = Subscription.NoSubscription;
                if (update.CallbackQuery.Data.Equals("subOglaf")) subscriptionType = Subscription.Oglaf;
                if (update.CallbackQuery.Data.Equals("subXkcd")) subscriptionType = Subscription.XKCD;
                if (update.CallbackQuery.Data.Equals("subCoinCM")) subscriptionType = Subscription.CoinCapMarket;

                var userId = update.CallbackQuery.From.Id;
                int.TryParse(userId, out int ID);
                using (AlcoDBEntities db = new AlcoDBEntities())
                {
                    var exists = DB.GetValue<int>(@"select top 1 c.id from Clients c 
                                        join Subscriptions s on s.id = c.subscription
                                        where c.chatid = " + userId +
                                        " and s.SubsctiptionType = " + ((int)subscriptionType).ToString());

                   

                    if (exists == 0)
                    {
                        var subscription = new Subscriptions { SubsctiptionType = (int)subscriptionType };
                        db.Subscriptions.Add(subscription);
                        db.SaveChanges();
                        var client = new Clients { ChatID = ID, Subscription = subscription.Id };
                        db.Clients.Add(client);
                        db.SaveChanges();

                        await bot.SendTextMessageAsync(userId, $"You've subscribed to { subscriptionType.ToString() }!");
                    }
                    else
                    {
                        //await bot.SendTextMessageAsync(userId, "Already subscribed");
                        var clients = db.Clients.Where(x => x.ChatID.ToString() == userId &&
                            db.Subscriptions.Any(y => y.Id == x.Subscription && y.SubsctiptionType == (int)subscriptionType));
                        foreach (var item in clients)
                        {
                            db.Clients.Remove(item);
                            var sub = db.Subscriptions.Where(x => x.Id == item.Subscription).FirstOrDefault();
                            db.Subscriptions.Remove(sub);
                        }
                        db.SaveChanges();
                        await bot.SendTextMessageAsync(userId, $"Unsubscribed from { subscriptionType.ToString() }!");
                    }

                }
                
            }

            //if (update.CallbackQuery.Data.StartsWith("-sub"))
            //{
            //    Subscription subscriptionType = Subscription.NoSubscription;
            //    if (update.CallbackQuery.Data.Equals("-subOglaf")) subscriptionType = Subscription.Oglaf;
            //    if (update.CallbackQuery.Data.Equals("-subXkcd")) subscriptionType = Subscription.XKCD;

            //    var userId = update.CallbackQuery.From.Id;
            //    int.TryParse(userId, out int ID);
            //    using (AlcoDBEntities db = new AlcoDBEntities())
            //    {
            //        if (db.Clients.Any(x => x.ChatID.ToString() == userId && db.Subscriptions.Any(y => y.Id == x.Subscription && y.SubsctiptionType ==(int)subscriptionType)))
            //        {
            //            var clients = db.Clients.Where(x => x.ChatID.ToString() == userId && 
            //                db.Subscriptions.Any(y => y.Id == x.Subscription && y.SubsctiptionType == (int)subscriptionType));
            //            foreach (var item in clients)
            //            {
            //                db.Clients.Remove(item);
            //                var sub = db.Subscriptions.Where(x => x.Id == item.Subscription).FirstOrDefault();
            //                db.Subscriptions.Remove(sub);
            //            }
            //            db.SaveChanges();
            //            await bot.SendTextMessageAsync(userId, $"Unsubscribed from { subscriptionType.ToString() }!");
            //        }
            //        else
            //        {
            //            await bot.SendTextMessageAsync(userId, "You are not subscribed");
            //        }

            //    }

            //}

            if (update.CallbackQuery.Data.Equals("vocabNW"))
            {
                var inlineKeyboardMarkup = new InlineKeyboardMarkup
                {
                    InlineKeyboard = new[]
                    {
                         new [] {  InlineKeyboardButton.WithCallbackData (
                                    "Next Word", "vocabNW"),
                                InlineKeyboardButton.WithCallbackData (
                                    "Definition", "vocabDefinition=" + VocabCallbackData.Word.WordText)
                        }
                    }
                };
                await bot.SendTextMessageAsync(update.CallbackQuery.From.Id, VocabCallbackData.Message, 
                    replyMarkup: inlineKeyboardMarkup, parseMode: ParseMode.Html);
                VocabCallbackData.PrepareNextWord();
            }

            if (update.CallbackQuery.Data.StartsWith("vocabDefinition"))
            {
                var word = update.CallbackQuery.Data.Substring(update.CallbackQuery.Data.IndexOf('=') + 1);
                await bot.SendTextMessageAsync(update.CallbackQuery.From.Id, 
                    VocabCallbackData.GetDefinition(word), parseMode: ParseMode.Html);
            }
        }
    }
}