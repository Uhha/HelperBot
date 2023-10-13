﻿using BotApi.Interfaces;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace BotApi.Services
{
    public class WebhookService : IWebhookService
    {
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
            var command = (_commands.ContainsKey(callbackPrefix)) ? _commands[callbackPrefix] : CommandType.Unknown;
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
            return (_commands.ContainsKey(callbackPrefix)) ? _commands[callbackPrefix] : CommandType.Unknown;

        }

        private static string ExtractCommand(Update update, bool isCallback = false)
        {
            var commandText = (isCallback) ? update.CallbackQuery.Data : update.Message.Text;
            commandText = commandText.Replace(' ', '=');
            commandText = (commandText.Contains('=')) ? commandText.Substring(0, commandText.IndexOf('=')) : commandText;
            return commandText;
        }

        public enum CommandType
        {
            ComicSubscribe,
            FincanceSubscribe,
            Coins,
            WakeOnLan,
            Balance,
            BalanceAdd,
            BalanceRemove,
            BalanceDetails,
            Unknown,
        }

        private static Dictionary<string, CommandType> _commands = new Dictionary<string, CommandType>
        {
            {"/subs", CommandType.ComicSubscribe },
            {"/finance", CommandType.FincanceSubscribe },
            {"/coins", CommandType.Coins },
            {"/c", CommandType.Coins },
            {"/wol", CommandType.WakeOnLan },
            {"/balance", CommandType.Balance },
            {"/b", CommandType.Balance },
            {"/balanceadd", CommandType.BalanceAdd },
            {"/balanceremove", CommandType.BalanceRemove },
            {"/balancedetails", CommandType.BalanceDetails },
            {"/bd", CommandType.BalanceDetails },
        };
    }
}