using DatabaseInteractions;
using Telegram.Bot;
using Tracer;

namespace Web.Services
{
    public class BotService : IBotService
    {
        public BotService()
        {
            TraceError.Info("Bot Service Init");
            Bot = new TelegramBotClient(Config.BotApiKey);
        }

        public TelegramBotClient Bot { get; }
    }
}