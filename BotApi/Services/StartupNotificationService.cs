using System.IO;
using BotApi.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types.Enums;

namespace BotApi.Services
{
    public class StartupNotificationService : IHostedService, IDisposable
    {
        private readonly ITelegramBotService _telegramBotService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<StartupNotificationService> _logger;

        public StartupNotificationService(
            ITelegramBotService telegramBotService,
            IConfiguration configuration,
            ILogger<StartupNotificationService> logger)
        {
            _telegramBotService = telegramBotService;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            // Get the admin chat ID from configuration
            var adminChatIdStr = _configuration["APIConfig:AdminChatId"];
            
            if (string.IsNullOrEmpty(adminChatIdStr))
            {
                _logger.LogWarning("AdminChatId not configured. Skipping startup notification.");
                return;
            }

            long adminChatId;
            if (!long.TryParse(adminChatIdStr, out adminChatId))
            {
                _logger.LogError($"Invalid AdminChatId format: {adminChatIdStr}");
                return;
            }

            // Get Docker image tag from VERSION.txt file or environment variable or use default
            var versionFile = Path.Combine(AppContext.BaseDirectory, "VERSION.txt");
            string dockerTag = string.Empty;
            
            if (File.Exists(versionFile))
            {
                try
                {
                    dockerTag = File.ReadAllText(versionFile).Trim() ?? "";
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to read VERSION.txt file.");
                }
            }

            // Clean up version string (remove 'v' prefix if present for cleaner display)
            string cleanVersion = (dockerTag.StartsWith("v")) ? dockerTag[1..] : dockerTag ?? "unknown";

            _logger.LogInformation($"Sending startup notification for version: {cleanVersion}");

            try
            {
                // Send notification message to admin chat
                await _telegramBotService.SendTextMessageAsync(
                    chatId: adminChatId,
                    message: $"🚀 New deployment detected!\n\n" +
                           $"📦 Version: <code>{cleanVersion}</code>\n" +
                           $"⏰ Time: {DateTime.Now:yyyy-MM-dd HH:mm:ss}",
                    parseMode: ParseMode.Html);

                _logger.LogInformation("Startup notification sent successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send startup notification.");
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            // Optionally send a shutdown message
            var adminChatIdStr = _configuration["APIConfig:AdminChatId"];
            if (!string.IsNullOrEmpty(adminChatIdStr) && long.TryParse(adminChatIdStr, out var adminChatId))
            {
                try
                {
                    await _telegramBotService.SendTextMessageAsync(
                        chatId: adminChatId,
                        message: "🔴 Bot is shutting down...",
                        parseMode: ParseMode.Html);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send shutdown notification.");
                }
            }
        }

        public void Dispose()
        {
            // Cleanup if needed
        }
    }
}
