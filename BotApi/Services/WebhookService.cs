using BotApi.Interfaces;
using Telegram.Bot;
using Telegram.Bot.Types;
using static BotApi.Commands.RegisterCommands;

namespace BotApi.Services
{
    public class WebhookService : IWebhookService
    {
        public WebhookService()
        {
        }

        public (CommandType command, bool isCallback) GetCommandType(Update update)
        {
            switch (update.Type)
            {
                case Telegram.Bot.Types.Enums.UpdateType.Message:
                    return MessageUpdate(update);
                case Telegram.Bot.Types.Enums.UpdateType.CallbackQuery:
                    return CallbackQuertUpdate(update);
                default:
                    return (CommandType.Unknown, false);
            }
        }

        private (CommandType, bool isCallback) CallbackQuertUpdate(Update update)
        {
            var callbackPrefix = ExtractCommand(update, true);
            var command = (CommandsText.ContainsKey(callbackPrefix)) ? CommandsText[callbackPrefix] : CommandType.Unknown;
            return (command, true);
        }

        private (CommandType, bool isCallback) MessageUpdate(Update update)
        {
            switch (update.Message.Type)
            {
                case Telegram.Bot.Types.Enums.MessageType.Text:
                    return (TextMessageUpdate(update), false);
                default:
                    return (CommandType.Unknown, false);
            }
        }

        private CommandType TextMessageUpdate(Update update)
        {
            if (string.IsNullOrEmpty(update.Message?.Text)) 
                return CommandType.Unknown;

            var callbackPrefix = ExtractCommand(update);
            return (CommandsText.ContainsKey(callbackPrefix)) ? CommandsText[callbackPrefix] : CommandType.Unknown;

        }

        private static string ExtractCommand(Update update, bool isCallback = false)
        {
            var commandText = (isCallback) ? update.CallbackQuery.Data : update.Message.Text;
            commandText = commandText.Replace(' ', '=');
            commandText = (commandText.Contains('=')) ? commandText.Substring(0, commandText.IndexOf('=')) : commandText;
            return commandText;
        }

       
    }
}
