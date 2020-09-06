using DatabaseInteractions;
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
using Tracer;

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

            TraceError.Info("Inside NoHookLoop");
            while (true)
            {
                var updates = await TelegramBotClient.GetUpdatesAsync(offset);

                foreach (var update in updates)
                {
                    TraceError.Info("Update from NoHookLoop");
                    await Task.Run(() => new MessageHandler().Handle(TelegramBotClient, update));
                    offset = update.Id + 1;
                }

                await Task.Delay(1000);
            }
        }

    }
}