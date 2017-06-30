using DatabaseInteractions;
using System;
using System.Linq;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Logic
{
    internal static class CallbackQueryCommandProcessor
    {
        internal static async void Process(TelegramBotClient bot, Update update)
        {
            if (update.CallbackQuery.Data.Equals("subOglaf"))
            {
                var userId = update.CallbackQuery.From.Id;
                int.TryParse(userId, out int ID);
                using (AlcoDBEntities db = new AlcoDBEntities())
                {
                    if (!db.Clients.Any(x => x.ChatID.ToString() == userId && x.Subscription == (int)Subscription.Oglaf))
                    {
                        var subscription = new Subscriptions { SubsctiptionType = (int)Subscription.Oglaf };
                        db.Subscriptions.Add(subscription);
                        db.SaveChanges();
                        var client = new Clients { ChatID = ID, Subscription = subscription.Id };
                        db.Clients.Add(client);
                        db.SaveChanges();

                        await bot.SendTextMessageAsync(userId, "You've subscribed to Oglaf!");
                    }
                    else
                    {
                        await bot.SendTextMessageAsync(userId, "Already subscribed");
                    }

                }
                
            }

            if (update.CallbackQuery.Data.Equals("-subOglaf"))
            {
                var userId = update.CallbackQuery.From.Id;
                int.TryParse(userId, out int ID);
                using (AlcoDBEntities db = new AlcoDBEntities())
                {
                    if (db.Clients.Any(x => x.ChatID.ToString() == userId && x.Subscription == (int)Subscription.Oglaf))
                    {
                        var clients = db.Clients.Where(x => x.ChatID.ToString() == userId && x.Subscription == (int)Subscription.Oglaf);
                        foreach (var item in clients)
                        {
                            db.Clients.Remove(item);
                            var sub = db.Subscriptions.Where(x => x.Id == item.Subscription).FirstOrDefault();
                            db.Subscriptions.Remove(sub);
                        }
                        db.SaveChanges();
                        await bot.SendTextMessageAsync(userId, "Unsubscribed from Oglaf!");
                    }
                    else
                    {
                        await bot.SendTextMessageAsync(userId, "You are not subscribed");
                    }

                }

            }
        }
    }
}