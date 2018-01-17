using DatabaseInteractions;
using Logic.Modules;
using Logic.Processors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using NLog;

namespace Logic.Handlers
{
    public class MessageHandler
    {
        private Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly TelegramBotClient _bot;

        public MessageHandler()
        {
            _bot = Bot.Get();
        }

        /// <summary>
        /// Update handling
        /// </summary>
        /// 
        public async void Handle(Update update)
        {
            _logger.Info(update);
            if (update == null)
            {
                _logger.Error("Unpdate is null");
                return;
            }
            switch (update.Type)
            {
                case Telegram.Bot.Types.Enums.UpdateType.UnknownUpdate:
                    break;
                case Telegram.Bot.Types.Enums.UpdateType.MessageUpdate:
                    await MessageUpdate(update);
                    break;
                case Telegram.Bot.Types.Enums.UpdateType.InlineQueryUpdate:
                    break;
                case Telegram.Bot.Types.Enums.UpdateType.ChosenInlineResultUpdate:
                    break;
                case Telegram.Bot.Types.Enums.UpdateType.CallbackQueryUpdate:
                    await CallbackQuertUpdate(update);
                    break;
                case Telegram.Bot.Types.Enums.UpdateType.EditedMessage:
                    break;
                case Telegram.Bot.Types.Enums.UpdateType.ChannelPost:
                    break;
                case Telegram.Bot.Types.Enums.UpdateType.EditedChannelPost:
                    break;
                case Telegram.Bot.Types.Enums.UpdateType.ShippingQueryUpdate:
                    break;
                case Telegram.Bot.Types.Enums.UpdateType.PreCheckoutQueryUpdate:
                    break;
                case Telegram.Bot.Types.Enums.UpdateType.All:
                    break;
                default:
                    break;
            }
            return;
        }

        private async Task CallbackQuertUpdate(Update update)
        {
            await CallbackQueryCommandProcessor.ProcessAsync(_bot, update);
        }

        private async Task MessageUpdate(Update update)
        {
            switch (update.Message.Type)
            {
                case Telegram.Bot.Types.Enums.MessageType.UnknownMessage:
                    break;
                case Telegram.Bot.Types.Enums.MessageType.TextMessage:
                    await TextMessageUpdate(update);
                    break;
                case Telegram.Bot.Types.Enums.MessageType.PhotoMessage:
                    break;
                case Telegram.Bot.Types.Enums.MessageType.AudioMessage:
                    break;
                case Telegram.Bot.Types.Enums.MessageType.VideoMessage:
                    break;
                case Telegram.Bot.Types.Enums.MessageType.VoiceMessage:
                    break;
                case Telegram.Bot.Types.Enums.MessageType.DocumentMessage:
                    DocumentMessageCommandProcessor.Process(_bot, update);
                    break;
                case Telegram.Bot.Types.Enums.MessageType.StickerMessage:
                    break;
                case Telegram.Bot.Types.Enums.MessageType.LocationMessage:
                    break;
                case Telegram.Bot.Types.Enums.MessageType.ContactMessage:
                    break;
                case Telegram.Bot.Types.Enums.MessageType.ServiceMessage:
                    break;
                case Telegram.Bot.Types.Enums.MessageType.VenueMessage:
                    break;
                case Telegram.Bot.Types.Enums.MessageType.GameMessage:
                    break;
                case Telegram.Bot.Types.Enums.MessageType.VideoNoteMessage:
                    break;
                case Telegram.Bot.Types.Enums.MessageType.Invoice:
                    break;
                case Telegram.Bot.Types.Enums.MessageType.SuccessfulPayment:
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
                await _bot.SendChatActionAsync(update.Message?.Chat?.Id, Telegram.Bot.Types.Enums.ChatAction.Typing);
                await TextMessageCommandProcessor.ProcessAsync(_bot, update);
            }


        }
    }
}