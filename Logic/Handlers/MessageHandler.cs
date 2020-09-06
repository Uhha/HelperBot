using DatabaseInteractions;
using Logic.Modules;
using Logic.Processors;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using Tracer;

namespace Logic.Handlers
{
    public class MessageHandler
    {
        /// <summary>
        /// Update handling
        /// </summary>
        /// 
        public async Task Handle(TelegramBotClient bot, Update update)
        {
            TraceError.Info($"inside message handler with {update?.Message?.Text}");
            if (update == null)
            {
                TraceError.Error("Update is null");
                return;
            }
            switch (update.Type)
            {
                case Telegram.Bot.Types.Enums.UpdateType.Message:
                    await MessageUpdate(bot, update);
                    break;
                case Telegram.Bot.Types.Enums.UpdateType.CallbackQuery:
                    await CallbackQuertUpdate(bot, update);
                    break;
                default:
                    break;
            }
            return;
        }

        private async Task CallbackQuertUpdate(TelegramBotClient bot, Update update)
        {
            await CallbackQueryCommandProcessor.ProcessAsync(bot, update);
        }

        private async Task MessageUpdate(TelegramBotClient bot, Update update)
        {
            switch (update.Message.Type)
            {
                case Telegram.Bot.Types.Enums.MessageType.Text:
                    await TextMessageUpdate(bot, update);
                    break;
                default:
                    break;
            }

            
        }

        private async Task TextMessageUpdate(TelegramBotClient bot, Update update)
        {
            if (string.IsNullOrEmpty(update.Message?.Text)) return;

            if (update.Message.Text.StartsWith("/"))
            {
                await bot.SendChatActionAsync(update.Message?.Chat?.Id, Telegram.Bot.Types.Enums.ChatAction.Typing);
                await TextMessageCommandProcessor.ProcessAsync(bot, update);
            }


        }
    }
}