using DatabaseInteractions;
using Logic.Grabbers;
using Logic.Processors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace Logic.Handlers
{
    public class WorkerHandler
    {
        private readonly TelegramBotClient _bot;

        public WorkerHandler()
        {
            _bot = Bot.Get();
        }

        public async void HandleComicAsync()
        {
            //Debug msg
            //await _bot.SendTextMessageAsync(182328439, "HandleComicCalled " + DateTime.Now.Minute + ":" + DateTime.Now.Second);

            var clients = DB.GetList<int>("select distinct c.chatId from Clients c " +
                "join Subscriptions s on s.id = c.subscription " +
                "where s.SubsctiptionType = " + (int)Subscription.Oglaf);

            foreach (var client in clients)
            {
                try
                {
                    var result = ComicGrabber.GetOglafPicture(client);
                    if (result.doSend)
                    {
                        await _bot.SendTextMessageAsync(client, result.alt.ToUpper());
                        await _bot.SendTextMessageAsync(client, result.title);
                        await _bot.SendPhotoAsync(client, new FileToSend(result.scr));
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
                    var result = ComicGrabber.GetXKCDPicture(client);
                    if (result.doSend)
                    {
                        await _bot.SendTextMessageAsync(client, result.alt.ToUpper());
                        await _bot.SendTextMessageAsync(client, result.title);
                        await _bot.SendPhotoAsync(client, new FileToSend(result.scr));
                    }
                }
            }
            catch (Exception)
            {

            }


            return;

        }

        public async void HandleCoinAsync(bool sendAnyway, int currenciesNumber = 5)
        {
            ////Debug msg
            //await _bot.SendTextMessageAsync(182328439, "HandleCoinsCalled " + DateTime.Now.Minute + ":" + DateTime.Now.Second);
            //if (sendAnyway)
            //{
            //    //Debug msg
            //    await _bot.SendTextMessageAsync(182328439, "MandatoryCoinsCalled " + DateTime.Now.Minute + ":" + DateTime.Now.Second);

            //}
            var result = await CoinGrabber.GetPricesAsync(currenciesNumber);
            if ((!sendAnyway && !result.Item2) || string.IsNullOrEmpty(result.Item1)) return;

            var clients = DB.GetList<int>("select distinct c.chatId from Clients c " +
                "join Subscriptions s on s.id = c.subscription " +
                "where s.SubsctiptionType = " + (int)Subscription.CoinCapMarket);

            foreach (var client in clients)
            {
                await _bot.SendTextMessageAsync(client, result.Item1, parseMode: Telegram.Bot.Types.Enums.ParseMode.Html);
            }
        }
    }
}