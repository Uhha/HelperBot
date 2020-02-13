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
            TraceError.Info("HandleComicAsync called from Worker");
            await new ComicModule().GenerateAndSendWorkerAsync(_bot);
        }

        public async void HandleCoinAsync(string sendAnyway)
        {
            TraceError.Info("HandleCoinAsync called from Worker");
            await new CoinModule().GenerateAndSendWorkerAsync(_bot, new List<string>() { sendAnyway });
        }

        public async void SendErrorMessageToBot(string errormsg)
        {
            TraceError.Info("HandleErrorMsg called from Worker");
            await new ErrorMsgModule().SendErrorMessage(_bot, errormsg);
        }

    }
}