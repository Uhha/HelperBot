using DatabaseInteractions;
using Logic.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Logic.Processors
{
    internal static class CallbackQueryCommandProcessor
    {
        private static Dictionary<string, CallbackCommand> _commands = new Dictionary<string, CallbackCommand>
        {
            {"/sub", CallbackCommand.ComicSubscribe },
            {"/vocabNewWord", CallbackCommand.VocabNewWord },
            {"/vocabDefinition", CallbackCommand.VocabDefinition },
        };

        internal static async Task ProcessAsync(TelegramBotClient bot, Update update)
        {
            var callbackPrefix = Helper.ExtractCommand(update, true);
            var command = (_commands.ContainsKey(callbackPrefix)) ? _commands[callbackPrefix] : CallbackCommand.Unknown;

            switch (command)
            {
                case CallbackCommand.ComicSubscribe:
                    await new ComicModule().GenerateAndSendCallbackAsync(bot, update);
                    break;
                case CallbackCommand.VocabNewWord:
                    await new VocabModule().GenerateAndSendCallbackAsync(bot, update);
                    break;
                case CallbackCommand.VocabDefinition:
                    await new VocabModule().GenerateAndSendDefineCallbackAsync(bot, update);
                    break;
                case CallbackCommand.Unknown:
                    break;
                default:
                    break;
            }
        }
    }
}