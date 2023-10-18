using BotApi.Interfaces;
using Telegram.Bot.Types;
using static BotApi.Commands.RegisterCommands;

namespace BotApi.Commands
{
    public class CommandInvoker
    {
        private readonly Dictionary<CommandType, ICommand> _commands;
        private readonly ICommandFactory _commandFactory;
        private readonly ITelegramBotService _telegramBotService;

        public CommandInvoker(Dictionary<CommandType, ICommand> commands, ICommandFactory commandFactory, ITelegramBotService telegramBotService)
        {
            _commands = commands;
            _commandFactory = commandFactory;
            _telegramBotService = telegramBotService;
        }

        public async Task ExecuteCommandAsync(CommandType commandType, Update update)
        {
            if (_commands.TryGetValue(commandType, out ICommand command))
            {
                if (command == null)
                {
                    // Use the factory to create the command
                    command = _commandFactory.Create(commandType);
                }

                if (command is IAsynchronousCommand asyncCommand)
                {
                    await asyncCommand.ExecuteAsync(update);
                }
                else if (command is ISynchronousCommand syncCommand)
                {
                    syncCommand.Execute(update);
                }
            }
            else
            {
                await _telegramBotService.ReplyAsync(update, $"Command {commandType} Not Found.");
            }
        }

        public void ExecuteCommand(CommandType commandType, Update update)
        {
            if (_commands.TryGetValue(commandType, out ICommand command))
            {
                if (command == null)
                {
                    // Use the factory to create the command
                    command = _commandFactory.Create(commandType);
                }

                if (command is IAsynchronousCommand asyncCommand)
                {
                    _ = asyncCommand.ExecuteAsync(update).ConfigureAwait(false);
                }
                else if (command is ISynchronousCommand syncCommand)
                {
                    syncCommand.Execute(update);
                }
            }
            else
            {
                _telegramBotService.ReplyAsync(update, $"Command {commandType} Not Found.");
            }
        }
    }
}
