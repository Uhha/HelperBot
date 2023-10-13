using BotApi.Interfaces;
using Telegram.Bot.Types;
using static BotApi.Program;

namespace BotApi.Commands
{
    public class CommandInvoker
    {
        private readonly Dictionary<string, ICommand> _commands;
        private readonly ICommandFactory _commandFactory;
        public CommandInvoker(Dictionary<string, ICommand> commands, ICommandFactory commandFactory)
        {
            _commands = commands;
            _commandFactory = commandFactory;
        }

        public async Task ExecuteCommandAsync(string commandString, Update update)
        {
            if (_commands.TryGetValue(commandString, out ICommand command))
            {
                if (command == null)
                {
                    // Use the factory to create the command
                    command = _commandFactory.Create(commandString);
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
                //TODO:
                // Handle unknown command
            }
        }

        public void ExecuteCommand(string commandString, Update update)
        {
            if (_commands.TryGetValue(commandString, out ICommand command))
            {
                if (command == null)
                {
                    // Use the factory to create the command
                    command = _commandFactory.Create(commandString);
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
                //TODO:
                // Handle unknown command
            }
        }
    }
}
