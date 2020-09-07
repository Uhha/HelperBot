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
        public async Task Handle(Update update)
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
                    await MessageUpdate(update);
                    break;
                case Telegram.Bot.Types.Enums.UpdateType.CallbackQuery:
                    await CallbackQuertUpdate(update);
                    break;
                default:
                    break;
            }
            return;
        }

        private async Task CallbackQuertUpdate(Update update)
        {
            await CallbackQueryCommandProcessor.ProcessAsync(update);
        }

        private async Task MessageUpdate(Update update)
        {
            switch (update.Message.Type)
            {
                case Telegram.Bot.Types.Enums.MessageType.Text:
                    await TextMessageUpdate(update);
                    break;
                default:
                    break;
            }

            
        }

        private async Task TextMessageUpdate(Update update)
        {
            if (string.IsNullOrEmpty(update.Message?.Text)) return;

            if (update.Message.Text.StartsWith("/"))
            {
                await Bot.Get().SendChatActionAsync(update.Message?.Chat?.Id, Telegram.Bot.Types.Enums.ChatAction.Typing);
                await TextMessageCommandProcessor.ProcessAsync(Bot.Get(), update);
            }


        }
    }
}