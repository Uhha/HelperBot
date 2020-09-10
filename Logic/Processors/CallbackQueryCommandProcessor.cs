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
            {"/subs", CallbackCommand.ComicSubscribe },
        };

        internal static async Task ProcessAsync(Update update)
        {
            var callbackPrefix = Helper.ExtractCommand(update, true);
            var command = (_commands.ContainsKey(callbackPrefix)) ? _commands[callbackPrefix] : CallbackCommand.Unknown;

            switch (command)
            {
                case CallbackCommand.ComicSubscribe:
                    await new ComicModule().UpdateSubscriptions(update);
                    break;
                case CallbackCommand.Unknown:
                    break;
                default:
                    break;
            }
        }
    }
}