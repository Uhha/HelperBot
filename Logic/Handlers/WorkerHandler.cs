using DatabaseInteractions;
using Logic.Modules;
using Logic.Processors;
using NLog;
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
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private readonly TelegramBotClient _bot;

        public WorkerHandler()
        {
            _bot = Bot.Get();
        }

        public async void HandleComicAsync()
        {
            await new ComicModule().GenerateAndSendWorkerAsync(_bot);
            logger.Info("HandleComicAsync called from Worker");
        }

        public async void HandleCoinAsync(string sendAnyway)
        {
            await new CoinModule().GenerateAndSendWorkerAsync(_bot, new List<string>() { sendAnyway } );
            logger.Info("HandleCoinAsync called from Worker");
        }

        public async void RecordCoinPrice()
        {
            new TrendModule().RecordCoinPrice();
            logger.Info("HandleCoinAsync called from Worker");
        }
    }
}