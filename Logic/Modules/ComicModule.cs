using DatabaseInteractions;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using Tracer;


[assembly: InternalsVisibleTo("BaseTests")]
namespace Logic.Modules
{
    internal class ComicModule
    {
        private TelegramBotClient _bot;

        public ComicModule()
        {
            _bot = Bot.Get();
        }

        public async Task ShowSubscriptionsButtons(Update update)
        {
            var inlineKeyboardMarkup = new InlineKeyboardMarkup
            (
                new[]
                    {
                    new [] {  InlineKeyboardButton.WithCallbackData ("Oglaf", "/subs=Oglaf"),
                             InlineKeyboardButton.WithCallbackData ("xkcd", "/subs=Xkcd"),
                             InlineKeyboardButton.WithCallbackData ("ErrorLogs", "/subs=ErrL")}
                    }
            );

            await _bot.SendTextMessageAsync(update.Message.Chat.Id, "Subscribe/Unsubscribe:", replyMarkup: inlineKeyboardMarkup);
        }

        public async Task UpdateSubscriptions(Update update)
        {
            Subscription subscriptionType = Subscription.NoSubscription;
            if (update.CallbackQuery.Data.Equals("/subs=Oglaf")) subscriptionType = Subscription.Oglaf;
            if (update.CallbackQuery.Data.Equals("/subs=Xkcd")) subscriptionType = Subscription.XKCD;
            if (update.CallbackQuery.Data.Equals("/subs=CoinCM")) subscriptionType = Subscription.CoinCapMarket;
            if (update.CallbackQuery.Data.Equals("/subs=ErrL")) subscriptionType = Subscription.ErrorMessageLog;

            var userId = update.CallbackQuery.From.Id;

            TraceError.Info("In callback");

            using (BotDBContext db = new BotDBContext())
            {
                try
                {
                    TraceError.Info("DB Init");
                    var exists = (from c in db.Clients
                                  join sub in db.Subscriptions on c.Subscription equals sub.Id
                                  where c.ChatId == userId
                                    && sub.SubsctiptionType == (int)subscriptionType
                                  select c.Id
                                     ).Count();



                    if (exists == 0)
                    {
                        var subscription = new DatabaseInteractions.Subscription { SubsctiptionType = (int)subscriptionType };
                        db.Subscriptions.Add(subscription);
                        db.SaveChanges();

                        //TODO: ChatId should be stored as long
                        var client = new DatabaseInteractions.Client { ChatId = (int)(userId), Subscription = subscription.Id };
                        db.Clients.Add(client);
                        db.SaveChanges();

                        await _bot.SendTextMessageAsync(userId, $"You've subscribed to { subscriptionType.ToString() }!");
                    }
                    else
                    {
                        var clients = db.Clients.Where(x => x.ChatId == userId &&
                            db.Subscriptions.Any(y => y.Id == x.Subscription && y.SubsctiptionType == (int)subscriptionType));
                        foreach (var item in clients)
                        {
                            db.Clients.Remove(item);
                            var sub = db.Subscriptions.Where(x => x.Id == item.Subscription).FirstOrDefault();
                            db.Subscriptions.Remove(sub);
                        }
                        db.SaveChanges();
                        await _bot.SendTextMessageAsync(userId, $"Unsubscribed from { subscriptionType.ToString() }!");
                    }
                }
                catch (Exception e)
                {
                    TraceError.Info(e.Message);
                }

            }
        }

        public async Task SendComicsAsync()
        {
            await SendComicsAsync(Subscription.Oglaf);
            await SendComicsAsync(Subscription.XKCD);
        }

        private async Task SendComicsAsync(Subscription subscriptionType)
        {
            using (var db = new BotDBContext())
            {
                var clients = (from c in db.Clients
                               join sub in db.Subscriptions on c.Subscription equals sub.Id
                               where sub.SubsctiptionType == (int)subscriptionType
                               select c.ChatId
                                       ).Distinct();

                MessageToSend message = (subscriptionType == Subscription.Oglaf) ? GetOglafPicture() : GetXKCDPicture();
                
                foreach (var client in clients)
                {
                    var lastPostedKey = (from cli in db.Clients
                                         join sub in db.Subscriptions on cli.Subscription equals sub.Id
                                         where cli.ChatId == client && sub.SubsctiptionType == (int)subscriptionType
                                         orderby sub.Id descending
                                         select new
                                         {
                                             LTK = sub.LastPostedKey,
                                             SUBID = sub.Id
                                         }
                        ).First();

                    if (message.Title.Equals(lastPostedKey.LTK)) continue;

                    DatabaseInteractions.Subscription subToUpdate = db.Subscriptions.Where(x => x.Id == lastPostedKey.SUBID).First();
                    string newHash = message.Title;
                    subToUpdate.LastPostedKey = newHash;
                    db.Update(subToUpdate);
                    //db.Subscriptions.Where(x => x.Id == lastPostedKey.SUBID).First().LastPostedKey = message.Title.GetHashCode().ToString();

                    try
                    {
                        await _bot.SendTextMessageAsync(client, message.Title.ToUpper());
                        await _bot.SendTextMessageAsync(client, message.SubTitle);
                        await _bot.SendPhotoAsync(client, new InputFileUrl(message.Image));
                    }
                    catch (Exception e)
                    {
                        TraceError.Info(e.Message);
                        var clientsRecords = db.Clients.Where(c => c.ChatId == client).ToList();
                        TraceError.Info("Client Recs to remove: " + string.Join(",", clientsRecords.Select(c => c.ChatId)));
                        var subscriptionsToRemove = db.Subscriptions.Where(x => clientsRecords.Select(o => o.Subscription).Contains(x.Id));
                        TraceError.Info("Subscription Recs to remove: " + string.Join(",", subscriptionsToRemove.Select(s => s.SubsctiptionType.ToString())));
                        db.Subscriptions.RemoveRange(subscriptionsToRemove);
                        db.Clients.RemoveRange(clientsRecords);
                    }
                }
                await db.SaveChangesAsync();
            }
        }

        private MessageToSend GetOglafPicture()
        {
            WebClient webclient = new WebClient();
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            string html = "";
            try
            {
                html = webclient.DownloadString("http://www.oglaf.com"); 
            }
            catch (Exception)
            {
                throw;
            }

            HtmlAttributeCollection attrs = null;
            try
            {
                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(html);

                List<HtmlNode> imageNodes = null;
                imageNodes = (from HtmlNode node in doc.DocumentNode.SelectNodes("//img")
                              where node.Name == "img"
                              && node.Attributes["id"]?.Value == "strip"
                              select node).ToList();
                attrs = imageNodes[0]?.Attributes;
            }
            catch (Exception)
            {
                throw;
            }

            return new MessageToSend { 
                Title = attrs["alt"]?.Value.Replace("&quot;", "\"").Replace("&#39;", "'"),
                SubTitle = attrs["title"]?.Value.Replace("&quot;", "\"").Replace("&#39;", "'"),
                Image = attrs["src"]?.Value
            };
        }

        private MessageToSend GetXKCDPicture()
        {
            WebClient webclient = new WebClient();
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            // Use SecurityProtocolType.Ssl3 if needed for compatibility reasons
            string html = "";
            try
            {
                 html = webclient.DownloadString("https://xkcd.com/");

            }
            catch (Exception e )
            {
                TraceError.Error(e);
            }

            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);

            List<HtmlNode> imageNodes = null;
            imageNodes = (from HtmlNode node in doc.DocumentNode.SelectNodes("//img")
                          where node.Name == "img"
                          && !string.IsNullOrEmpty(node.Attributes["title"]?.Value)
                          && node.Attributes["src"].Value.Contains("comics")
                          select node).ToList();

            var attrs = imageNodes[0]?.Attributes;

            return new MessageToSend
            {
                Title = attrs["alt"]?.Value.Replace("&quot;", "\"").Replace("&#39;", "'"),
                SubTitle = attrs["title"]?.Value.Replace("&quot;", "\"").Replace("&#39;", "'"),
                Image = attrs["src"]?.Value.Substring(2)
            };
        }

        private class MessageToSend
        {
            public string Title;
            public string SubTitle;
            public string Image;
        }
       
    }
}
