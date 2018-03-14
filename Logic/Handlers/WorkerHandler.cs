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
            await new CoinModule().GenerateAndSendWorkerAsync(_bot, new List<string>() { sendAnyway } );
            TraceError.Info("HandleCoinAsync called from Worker");
        }

        public async void RecordCoinPrice()
        {
            new TrendModule().RecordCoinPrice();
            TraceError.Info("HandleCoinAsync called from Worker");
        }

        public async void RemoveOldRecords()
        {
            new TrendModule().GenerateAndSendWorkerAsync(_bot);
        }
    }
}