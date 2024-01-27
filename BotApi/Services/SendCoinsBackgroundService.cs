
using BotApi.Database;
using BotApi.Interfaces;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types;

namespace BotApi.Services
{
    public class SendCoinsBackgroundService : BackgroundService
    {
        private readonly ILogger<SendCoinsBackgroundService> _logger;
        private IGetCoinsService _getCoinsService;
        private IDB _db;
        private ITelegramBotService _telegramBotService;
        private readonly Timer _timer;

        public SendCoinsBackgroundService(
            ILogger<SendCoinsBackgroundService> logger, 
            IGetCoinsService getCoinsService,
            IDB db,
            ITelegramBotService telegramBotService
            )
        {
            _logger = logger;
            _getCoinsService = getCoinsService;
            _db = db;
            _telegramBotService = telegramBotService;

            // Calculate the time until the next occurrence of 11:00 AM
            var now = DateTime.Now;
            var nextRunTime = new DateTime(now.Year, now.Month, now.Day, 11, 0, 0);
            if (now >= nextRunTime)
            {
                nextRunTime = nextRunTime.AddDays(1);
            }

            var delay = nextRunTime - now;

            // Set up a timer to trigger at 11:00 AM every day
            _timer = new Timer(ExecuteDailyTask, null, delay, TimeSpan.FromDays(1));
        }

        private async void ExecuteDailyTask(object state)
        {
            _logger.LogInformation("Running daily SendCoinsBackgroundService...");

            var prices = await _getCoinsService.GetPricesAsync(5);

            foreach (var client in _db.GetClientsWithSubscription(SubscriptionType.CoinCapMarket))
            {
                if (long.TryParse(client, out long chatId))
                {
                    await _telegramBotService.SendTextMessageAsync(chatId, prices.Item1, parseMode: ParseMode.Html);
                }
            }
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Change(Timeout.Infinite, 0);
            return base.StopAsync(cancellationToken);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return Task.CompletedTask;
        }
    }
}
