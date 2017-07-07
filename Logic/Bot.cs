using Telegram.Bot;

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
            _bot = new TelegramBotClient(Config.BotApiKey);
            _bot.SetWebhookAsync(Config.WebHookUrl);
            return _bot;
		}
    }
}