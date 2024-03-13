
using BotApi.Database;
using BotApi.Interfaces;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types;

namespace BotApi.Services
{
    public class SendSecuritiesBackgroundService : BackgroundService
    {
        private readonly ILogger<SendSecuritiesBackgroundService> _logger;
        private ISecuritiesService _securitiesService;
        private IServiceScopeFactory _scopeFactory;
        private ITelegramBotService _telegramBotService;
        private readonly Timer _timer;

        public SendSecuritiesBackgroundService(
            ILogger<SendSecuritiesBackgroundService> logger, 
            ISecuritiesService securitiesService,
            IServiceScopeFactory scopeFactory,
            ITelegramBotService telegramBotService
            )
        {
            _logger = logger;
            _securitiesService = securitiesService;
            _scopeFactory = scopeFactory;
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
            _logger.LogInformation("Running daily SendSecuritiesBackgroundService...");


            using (var scope = _scopeFactory.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<IDB>();
                foreach (var client in db.GetClientsWithSubscription(SubscriptionType.SecuritiesPrices))
                {
                    if (long.TryParse(client, out long chatId))
                    {
                        var prices = await _securitiesService.GetPricesAsync(chatId);
                        await _telegramBotService.SendTextMessageAsync(chatId, string.Join(Environment.NewLine, prices), parseMode: ParseMode.Html);
                    }
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
