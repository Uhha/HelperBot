using DatabaseInteractions;
using Logic.Modules;
using Logic.Processors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using Tracer;

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
            await new ComicModule().GenerateAndSendWorkerAsync(_bot);
            TraceError.Info("HandleComicAsync called from Worker");
        }

        public async void HandleCoinAsync(string sendAnyway)
        {
            try
            {
                await new CoinModule().GenerateAndSendWorkerAsync(_bot, new List<string>() { sendAnyway });
            }
            catch (Exception e)
            {
                TraceError.Error(e);
            }
            TraceError.Info("HandleCoinAsync called from Worker");
        }
        
    }
}