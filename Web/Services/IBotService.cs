using Telegram.Bot;

namespace Web.Services
{
    public interface IBotService
    {
        TelegramBotClient Bot { get; }
    }
}