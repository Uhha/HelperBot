﻿using DatabaseInteractions;
using Logic.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Tracer;

namespace Logic.Processors
{
    internal static class TextMessageCommandProcessor
    {
        private static Dictionary<string, Command> _commands = new Dictionary<string, Command>
        {
            {"/subs", Command.ComicSubscribe },
            {"/finance", Command.FincanceSubscribe },
            {"/coins", Command.Coins },
            {"/c", Command.Coins },
            {"/wol", Command.WakeOnLan },
            {"/balance", Command.Balance },
            {"/b", Command.Balance },
            {"/balanceadd", Command.BalanceAdd },
            {"/balanceremove", Command.BalanceRemove },
            {"/balancedetails", Command.BalanceDetails },
            {"/bd", Command.BalanceDetails },
        };

        internal static async Task ProcessAsync(TelegramBotClient bot, Update update)
        {
            var callbackPrefix = Helper.ExtractCommand(update);
            var command = (_commands.ContainsKey(callbackPrefix)) ? _commands[callbackPrefix] : Command.Unknown;

            switch (command)
            {
                case Command.ComicSubscribe:
                    await new ComicModule().ShowSubscriptionsButtons(update);
                    break;
                case Command.FincanceSubscribe:
                    await new FinanceModule().GenerateAndSendAsync(bot, update);
                    break;
                case Command.Coins:
                    await new CoinModule().GenerateAndSendAsync(bot, update);
                    break;
                case Command.WakeOnLan:
                    await new WakeOnLanModule().GenerateAndSendAsync(bot, update);
                    break;
                case Command.Balance:
                    await new BalanceModule().GenerateAndSendAsync(bot, update);
                    break;
                case Command.BalanceAdd:
                    await new BalanceModule().BalanceAddAsync(bot, update);
                    break;
                case Command.BalanceRemove:
                    await new BalanceModule().BalanceRemoveAsync(bot, update);
                    break;
                case Command.BalanceDetails:
                    await new BalanceModule().BalanceDetailsAsync(bot, update);
                    break;
                case Command.Unknown:
                    break;
                default:
                    break;
            }
        }
    }
}