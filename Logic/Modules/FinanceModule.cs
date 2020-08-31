using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace Logic.Modules
{
    class FinanceModule : IModule
    {
        public async Task GenerateAndSendAsync(TelegramBotClient bot, Update update)
        {
            var inlineKeyboardMarkup = new InlineKeyboardMarkup
            (
                new[]
                    {
                        new [] {  InlineKeyboardButton.WithCallbackData ("CoinCapMarket", "sub=CoinCM") }
                    }
            );
            await bot.SendTextMessageAsync(update.Message.Chat.Id, "Choose what to subscribe to:", replyMarkup: inlineKeyboardMarkup);
        }

        public Task GenerateAndSendCallbackAsync(TelegramBotClient bot, Update update)
        {
            throw new NotImplementedException();
        }

        public Task GenerateAndSendWorkerAsync(TelegramBotClient bot, IList<string> parameters = null)
        {
            throw new NotImplementedException();
        }
    }
}
