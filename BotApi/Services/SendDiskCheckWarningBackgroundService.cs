using BotApi.Interfaces;
using Microsoft.Extensions.Options;
using QBittorrent.Client;
using System.Text;
using Telegram.Bot.Types;

namespace BotApi.Services
{
    public class SendDiskCheckWarningBackgroundService : IHostedService, IDisposable
    {
        private readonly ITelegramBotService _telegramBotService;
        private readonly IOptions<APIConfig> _apiConfig;
        private readonly ILogger<TorrentStatusCheckService> _logger;
        private Timer _timer;

        public SendDiskCheckWarningBackgroundService(ITelegramBotService telegramBotService, IOptions<APIConfig> apiConfig, ILogger<TorrentStatusCheckService> logger)
        {
            _telegramBotService = telegramBotService;
            _apiConfig = apiConfig;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"SendDiskCheckWarningService StartAsync ran.");
            
            _timer = new Timer(async state => await GetLowDiskSpaceWarning(), null, TimeSpan.Zero, TimeSpan.FromHours(1));
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        private string[] _includeDrives =
        {
            "/",
            "/app/logs"
        };

        private async Task GetLowDiskSpaceWarning()
        {
            var drives = DriveInfo.GetDrives();
            var sb = new StringBuilder();

            foreach (var drive in drives)
            {
                if (drive.IsReady && drive.DriveType == DriveType.Fixed && _includeDrives.Contains(drive.Name))
                {
                    // Calculate percentage of free space
                    double freeSpacePercentage = ((double)drive.AvailableFreeSpace / drive.TotalSize) * 100;

                    // Send warning if free space is less than 5%
                    if (freeSpacePercentage < 5)
                    {
                        sb.AppendLine($"⚠️ *Warning!* Low disk space on {drive.Name}");
                        sb.AppendLine($"  - *Free Space:* {FormatBytes(drive.AvailableFreeSpace)} ({freeSpacePercentage:0.##}%)");
                        sb.AppendLine();
                    }
                }
            }

            if (sb.Length > 0 && _apiConfig.Value.AdminChatId is not null)
                await _telegramBotService.ReplyAsync(_apiConfig.Value.AdminChatId ?? 0, sb.ToString());
        }

        private string FormatBytes(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            double len = bytes;
            int order = 0;

            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len /= 1024;
            }

            return $"{len:0.##} {sizes[order]}";
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }

    }

}
