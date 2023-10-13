using Telegram.Bot.Types;

namespace BotApi.Interfaces
{
    public interface ICommand
    {
    }

    public interface ISynchronousCommand : ICommand
    {
        void Execute(Update update);
    }
    
    public interface IAsynchronousCommand : ICommand
    {
        Task ExecuteAsync(Update update);
    }
}
