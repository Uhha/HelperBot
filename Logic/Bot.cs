using Telegram.Bot;
using Tracer;

namespace Logic
{
    public static class Bot
    {
        private static TelegramBotClient _bot;
        /// <summary>
        /// Get the bot and set the hook
        /// </summary>
        public static TelegramBotClient Get()
        {
            if (_bot != null) return _bot;
            if (Config.Environment == "Debug")
            {
                _bot = new TelegramBotClient(Config.BotApiKey);
                _bot.SendTextMessageAsync(182328439, "Local Bot started!");
            }
            else
            {
                _bot = new TelegramBotClient(Config.BotApiKey);
                _bot.SetWebhookAsync(Config.WebHookUrl);
                _bot.SendTextMessageAsync(182328439, "Remote Bot started!");
                TraceError.Info("Bot started");
            }
            return _bot;
		}
    }
}