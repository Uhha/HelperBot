using BotApi.Interfaces;
using Microsoft.Extensions.Options;
using QBittorrent.Client;
using System.Text;
using Telegram.Bot.Types;

namespace BotApi.Services
{
    public class SendTempCheckWarningBackgroundService : IHostedService, IDisposable
    {
        private readonly ITelegramBotService _telegramBotService;
        private readonly IOptions<APIConfig> _apiConfig;
        private readonly ILogger<TorrentStatusCheckService> _logger;
        private Timer _timer;
        private static readonly TimeSpan RegularInterval = TimeSpan.FromMinutes(10);
        private static readonly TimeSpan AlertInterval = TimeSpan.FromMinutes(1);


        public SendTempCheckWarningBackgroundService(ITelegramBotService telegramBotService, IOptions<APIConfig> apiConfig, ILogger<TorrentStatusCheckService> logger)
        {
            _telegramBotService = telegramBotService;
            _apiConfig = apiConfig;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"SendTempCheckWarningBackgroundService StartAsync ran.");
            
            _timer = new Timer(async state => await GetHighTempWarning(), null, TimeSpan.Zero, RegularInterval);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        private async Task GetHighTempWarning()
        {
            var message = GetTemperatureWarningFallback();

            if (message is not null && _apiConfig.Value.AdminChatId is not null)
                await _telegramBotService.ReplyAsync(_apiConfig.Value.AdminChatId ?? 0, message);
        }

        private string? GetTemperatureWarningFallback(double thresholdCelsius = 75.0)
        {
            string temperatureFilePath = "/sys/class/thermal/thermal_zone0/temp";
            var sb = new StringBuilder();

            if (System.IO.File.Exists(temperatureFilePath))
            {
                string tempString = System.IO.File.ReadAllText(temperatureFilePath).Trim();
                if (double.TryParse(tempString, out double temperatureMillidegrees))
                {
                    double temperatureCelsius = temperatureMillidegrees / 1000.0;
                    if (temperatureCelsius > thresholdCelsius)
                    {
                        _timer?.Change(TimeSpan.Zero, AlertInterval);
                        sb.AppendLine($"⚠️ *Warning!* High CPU temperature detected!");
                        sb.AppendLine($"  - *Temperature:* {temperatureCelsius:0.##} °C");
                        return sb.ToString();
                    }
                    _timer?.Change(TimeSpan.Zero, RegularInterval);
                }
            }
            return null;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }

    }

}
