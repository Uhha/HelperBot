﻿using DatabaseInteractions;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.InlineKeyboardButtons;
using Telegram.Bot.Types.ReplyMarkups;

namespace Logic.Modules
{
    internal class ComicModule : IModule
    {
        public async Task GenerateAndSendAsync(TelegramBotClient bot, Update update)
        {
            var inlineKeyboardMarkup = new InlineKeyboardMarkup
            {
                InlineKeyboard = new[]
                    {
                    new [] {  InlineKeyboardButton.WithCallbackData ("Oglaf", "sub=Oglaf"),
                             InlineKeyboardButton.WithCallbackData ("xkcd", "sub=Xkcd")}
                }
            };
            await bot.SendTextMessageAsync(update.Message.Chat.Id, "Choose what to subscribe to:", replyMarkup: inlineKeyboardMarkup);
        }

        public async Task GenerateAndSendCallbackAsync(TelegramBotClient bot, Update update)
        {
            Subscription subscriptionType = Subscription.NoSubscription;
            if (update.CallbackQuery.Data.Equals("sub=Oglaf")) subscriptionType = Subscription.Oglaf;
            if (update.CallbackQuery.Data.Equals("sub=Xkcd")) subscriptionType = Subscription.XKCD;
            if (update.CallbackQuery.Data.Equals("sub=CoinCM")) subscriptionType = Subscription.CoinCapMarket;

            var userId = update.CallbackQuery.From.Id;
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
                    var client = new Clients { ChatID = userId, Subscription = subscription.Id };
                    db.Clients.Add(client);
                    db.SaveChanges();

                    await bot.SendTextMessageAsync(userId, $"You've subscribed to { subscriptionType.ToString() }!");
                }
                else
                {
                    //await bot.SendTextMessageAsync(userId, "Already subscribed");
                    var clients = db.Clients.Where(x => x.ChatID == userId &&
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

        public async Task GenerateAndSendWorkerAsync(TelegramBotClient bot, IList<string> parameters = null)
        {
            var clients = DB.GetList<int>("select distinct c.chatId from Clients c " +
                "join Subscriptions s on s.id = c.subscription " +
                "where s.SubsctiptionType = " + (int)Subscription.Oglaf);

            foreach (var client in clients)
            {
                try
                {
                    var result = GetOglafPicture(client);
                    if (result.doSend)
                    {
                        await bot.SendTextMessageAsync(client, result.alt.ToUpper());
                        await bot.SendTextMessageAsync(client, result.title);
                        await bot.SendPhotoAsync(client, new FileToSend(result.scr));
                    }
                    else
                    {
                        //await _bot.SendTextMessageAsync(myChatId, "Already there" + DateTime.Now.Minute + ":" + DateTime.Now.Second);
                    }
                }
                catch (Exception)
                {
                    //await _bot.SendTextMessageAsync(myChatId, ex.Message + Environment.NewLine + ex.InnerException);
                }
            }

            clients = DB.GetList<int>("select distinct c.chatId from Clients c " +
                "join Subscriptions s on s.id = c.subscription " +
                "where s.SubsctiptionType = " + (int)Subscription.XKCD);
            try
            {
                foreach (var client in clients)
                {
                    var result = GetXKCDPicture(client);
                    if (result.doSend)
                    {
                        await bot.SendTextMessageAsync(client, result.alt.ToUpper());
                        await bot.SendTextMessageAsync(client, result.title);
                        await bot.SendPhotoAsync(client, new FileToSend(result.scr));
                    }
                }
            }
            catch (Exception)
            {

            }
        }

        private static (bool doSend, string alt, string title, string scr) GetOglafPicture(int client)
        {
            WebClient webclient = new WebClient();
            string html = webclient.DownloadString("http://www.oglaf.com");


            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);

            List<HtmlNode> imageNodes = null;
            imageNodes = (from HtmlNode node in doc.DocumentNode.SelectNodes("//img")
                          where node.Name == "img"
                          && node.Attributes["id"]?.Value == "strip"
                          select node).ToList();

            var attrs = imageNodes[0]?.Attributes;

            return (
                NotSent(client, attrs["alt"]?.Value, Subscription.Oglaf),
                attrs["alt"]?.Value,
                attrs["title"]?.Value,
                attrs["src"]?.Value
                );
        }

        private static (bool doSend, string alt, string title, string scr) GetXKCDPicture(int client)
        {
            WebClient webclient = new WebClient();
            string html = webclient.DownloadString("https://xkcd.com/");


            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);

            List<HtmlNode> imageNodes = null;
            imageNodes = (from HtmlNode node in doc.DocumentNode.SelectNodes("//img")
                          where node.Name == "img"
                          && !string.IsNullOrEmpty(node.Attributes["title"]?.Value)
                          && node.Attributes["src"].Value.Contains("comics")
                          select node).ToList();

            var attrs = imageNodes[0]?.Attributes;

            return (
                NotSent(client, attrs["alt"]?.Value, Subscription.XKCD),
                attrs["alt"]?.Value,
                attrs["title"]?.Value,
                attrs["src"]?.Value.Substring(2)
                );
        }

        private static bool NotSent(int client, string alt, Subscription subscription)
        {
            using (AlcoDBEntities db = new AlcoDBEntities())
            {
                var lastPostedKey = (from cli in db.Clients
                           join sub in db.Subscriptions on cli.Subscription equals sub.Id
                           where cli.ChatID == client && sub.SubsctiptionType == (int)subscription
                           orderby sub.Id descending
                           select new
                           {
                               LTK = sub.LastPostedKey,
                               SUBID = sub.Id
                           }
                           ).First();

                if (alt.GetHashCode().ToString().Equals(lastPostedKey.LTK)) return false;
                db.Subscriptions.Where(x => x.Id == lastPostedKey.SUBID).First().LastPostedKey = alt.GetHashCode().ToString();
                db.SaveChanges();
            }
            return true;
        }

       
    }
}