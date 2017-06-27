using Logic.Oglaf;
using System;
using System.Collections.Generic;
using System.Linq;
using Telegram.Bot;
using Telegram.Bot.Types;

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
        /// Обрабатывает сообщение от пользователя
        /// </summary>
        public async void Handle(Message message)
        {
			await _bot.SendTextMessageAsync(message.Chat.Id, "WEBHOOK works" );
			return;

			var text = message.Text.Split(' ');
            if (text.First() != "/wol") return;
            switch (text.Count())
            {
                case 1:
                case 2:
                    await _bot.SendTextMessageAsync(message.Chat.Id, "Пример использования: /wol 1.2.3.4 01:02:03:04:05:06 7");
                    break;
                default:
                    if (!WakeOnLan.ValidateMac(text[2]))
                        await _bot.SendTextMessageAsync(message.Chat.Id, "Неверный MAC адрес");
                    else
                    {
                        try
                        {
                            WakeOnLan.Up(text[1], text[2], GetPort(text));
                            await _bot.SendTextMessageAsync(message.Chat.Id, "Пакет отправлен!");
                        }
                        catch (Exception)
                        {
                            await _bot.SendTextMessageAsync(message.Chat.Id, "Произошла ошибка :(");
                        }
                    }
                    break;
            }
        }


        private int myChatId = 182328439;
        public async void Handle()
        {
            //await _bot.SendTextMessageAsync(myChatId, "OGLAF WORKS");
            //await _bot.SendPhotoAsync(myChatId, new FileToSend("http://media.oglaf.com/comic/princess_party.jpg"));
            var result = OglafGrabber.GetOglafPicture(myChatId);
            if (result.doSend)
            {
                await _bot.SendTextMessageAsync(myChatId, result.alt.ToUpper());
                await _bot.SendTextMessageAsync(myChatId, result.title);
                await _bot.SendPhotoAsync(myChatId, new FileToSend(result.scr));
            }
            else
            {
               // await _bot.SendTextMessageAsync(myChatId, "Already there");
            }

            return;

        }

            /// <summary>
            /// Получаем порт из параметров
            /// </summary>
            private static int? GetPort(IReadOnlyList<string> text)
        {
            int port;
            if (text.Count == 4 && int.TryParse(text[3], out port))
                return port;
            return null;
        }
    }
}