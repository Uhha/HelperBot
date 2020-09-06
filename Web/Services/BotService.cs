using DatabaseInteractions;
using Telegram.Bot;

namespace Web.Services
{
    public class BotService : IBotService
    {
        public BotService()
        {
            Bot = new TelegramBotClient(Config.BotApiKey);
        }

        public TelegramBotClient Bot { get; }
    }
}