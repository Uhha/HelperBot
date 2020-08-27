using Logic;
using Logic.Handlers;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using Telegram.Bot;

namespace Web
{
    public static class NoHookLoop
    {
        public static void Start()
        {
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            Run().Wait();
        }

        static async Task Run()
        {
            var TelegramBotClient = new TelegramBotClient(Config.BotApiKey);

            var offset = 0;
            
            while (true)
            {
                var updates = await TelegramBotClient.GetUpdatesAsync(offset);

                foreach (var update in updates)
                {
                    await Task.Run(() => new MessageHandler().Handle(update));
                    offset = update.Id + 1;
                }

                await Task.Delay(1000);
            }
        }

    }
}