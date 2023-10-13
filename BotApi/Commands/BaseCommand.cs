using BotApi.Interfaces;
using Telegram.Bot.Types;

namespace BotApi.Commands
{
    public abstract class BaseCommand : ISynchronousCommand
    {
        internal readonly ITelegramBotService _telegramBotService;

        public BaseCommand(ITelegramBotService telegramBotService)
        {
            _telegramBotService = telegramBotService;
        }

        public abstract void Execute(Update update);
    }

    public abstract class BaseCommandAsync : IAsynchronousCommand
    {
        internal readonly ITelegramBotService _telegramBotService;

        public BaseCommandAsync(ITelegramBotService telegramBotService)
        {
            _telegramBotService = telegramBotService;
        }

        public abstract Task ExecuteAsync(Update update);
    }
}
