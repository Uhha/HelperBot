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
using NLog;

namespace Logic.Processors
{
    internal static class TextMessageCommandProcessor
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();

        private static Dictionary<string, Command> _commands = new Dictionary<string, Command>
        {
            {"/subs", Command.ComicSubscribe },
            {"/finance", Command.FincanceSubscribe },
            {"/coins", Command.Coins },
            {"/vocab", Command.Vocabulary },
            {"/define", Command.DefineWord },
            {"/wol", Command.WakeOnLan },
            {"/trend", Command.Trend }
        };

        internal static async Task ProcessAsync(TelegramBotClient bot, Update update)
        {
            var callbackPrefix = Helper.ExtractCommand(update);
            var command = (_commands.ContainsKey(callbackPrefix)) ? _commands[callbackPrefix] : Command.Unknown;

            switch (command)
            {
                case Command.ComicSubscribe:
                    await new ComicModule().GenerateAndSendAsync(bot, update);
                    break;
                case Command.FincanceSubscribe:
                    await new FinanceModule().GenerateAndSendAsync(bot, update);
                    break;
                case Command.Coins:
                    try
                    {
                        await new CoinModule().GenerateAndSendAsync(bot, update);
                    }
                    catch (Exception e)
                    {
                        _logger.Error(e);
                        await bot.SendTextMessageAsync(update.Message.Chat.Id, e.Message);
                        //throw;
                    }
                    break;
                case Command.Vocabulary:
                    await new VocabModule().GenerateAndSendAsync(bot, update);
                    break;
                case Command.DefineWord:
                    await new VocabModule().GenerateAndSendDefineAsync(bot, update);
                    break;
                case Command.WakeOnLan:
                    await new WakeOnLanModule().GenerateAndSendAsync(bot, update);
                    break;
                case Command.Unknown:
                    break;
                default:
                    break;
            }
        }
    }
}