using BotApi.Interfaces;
using Microsoft.Extensions.Options;
using QBittorrent.Client;
using System.Text;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

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

        private static readonly string _temperatureFilePath = "/sys/class/thermal/thermal_zone0/temp";
        private static readonly double _thresholdCelsius = 75.0;
        private static int? _lastMessageId;
        private static DateTime _dateSent = DateTime.MinValue;

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
            var newMessage = GetTemperatureWarningFallback();

            if (newMessage is null || _apiConfig.Value.AdminChatId is null)
                return;

            int minutesPassed = (int)(DateTime.Now - _dateSent).TotalMinutes;

            if (minutesPassed < 5 && _lastMessageId.HasValue)
            {
                await _telegramBotService.EditMessageAsync(_apiConfig.Value.AdminChatId, _lastMessageId.Value, newMessage, ParseMode.Markdown);
            }
            else
            {
                var message = await _telegramBotService.ReplyAsync(_apiConfig.Value.AdminChatId ?? 0, newMessage);
                _lastMessageId = message.MessageId;
                _dateSent = message.Date;
            }
        }

        private string? GetTemperatureWarningFallback()
        {
            var sb = new StringBuilder();

            if (System.IO.File.Exists(_temperatureFilePath))
            {
                string tempString = System.IO.File.ReadAllText(_temperatureFilePath).Trim();
                if (double.TryParse(tempString, out double temperatureMillidegrees))
                {
                    double temperatureCelsius = temperatureMillidegrees / 1000.0;
                    if (temperatureCelsius > _thresholdCelsius)
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
