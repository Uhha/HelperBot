using DatabaseInteractions;
using Logic.Oglaf;
using Logic.Processors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace Logic
{
    public class Handler
    {
        private readonly TelegramBotClient _bot;

        public Handler()
        {
            _bot = Bot.Get();
        }

        /// <summary>
        /// Update handling
        /// </summary>
        public async void Handle(Update update)
        {
            switch (update.Type)
            {
                case Telegram.Bot.Types.Enums.UpdateType.UnknownUpdate:
                    break;
                case Telegram.Bot.Types.Enums.UpdateType.MessageUpdate:
                    MessageUpdate(update);
                    break;
                case Telegram.Bot.Types.Enums.UpdateType.InlineQueryUpdate:
                    break;
                case Telegram.Bot.Types.Enums.UpdateType.ChosenInlineResultUpdate:
                    break;
                case Telegram.Bot.Types.Enums.UpdateType.CallbackQueryUpdate:
                    CallbackQuertUpdate(update);
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
            CallbackQueryCommandProcessor.Process(_bot, update);
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
            if (update.Message.Text.StartsWith("/"))
            {
                TextMessageCommandProcessor.ProcessAsync(_bot, update);
            }

        }

        public async void Handle()
        {
            //await _bot.SendTextMessageAsync(182328439, "Ping " + DateTime.Now.Minute + ":" + DateTime.Now.Second);

            var clients = DB.GetList<int>("select distinct c.chatId from Clients c " +
                "join Subscriptions s on s.id = c.subscription " +
                "where s.SubsctiptionType = " + (int)Subscription.Oglaf);

            foreach (var client in clients)
            {
                try
                {
                    var result = OglafGrabber.GetOglafPicture(client);
                    if (result.doSend)
                    {
                        await _bot.SendTextMessageAsync(client, result.alt.ToUpper());
                        await _bot.SendTextMessageAsync(client, result.title);
                        await _bot.SendPhotoAsync(client, new FileToSend(result.scr));
                    }
                    else
                    {
                        //await _bot.SendTextMessageAsync(myChatId, "Already there" + DateTime.Now.Minute + ":" + DateTime.Now.Second);
                    }
                }
                catch (Exception)
                {
                    //await _bot.SendTextMessageAsync(myChatId, ex.Message + Environment.NewLine + ex.InnerException);
                }
            }

            clients = DB.GetList<int>("select distinct c.chatId from Clients c " +
                "join Subscriptions s on s.id = c.subscription " +
                "where s.SubsctiptionType = " + (int)Subscription.XKCD);
            try
            {
                foreach (var client in clients)
                {
                    var result = OglafGrabber.GetXKCDPicture(client);
                    if (result.doSend)
                    {
                        await _bot.SendTextMessageAsync(client, result.alt.ToUpper());
                        await _bot.SendTextMessageAsync(client, result.title);
                        await _bot.SendPhotoAsync(client, new FileToSend(result.scr));
                    }
                }
            }
            catch (Exception)
            {

            }


            return;

        }
    }
}