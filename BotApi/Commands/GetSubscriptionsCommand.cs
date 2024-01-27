using BotApi.Database;
using BotApi.Interfaces;
using BotApi.Services;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace BotApi.Commands
{
    public class GetSubscriptionsCommand : BaseCommandAsync
    {
        private IDB _db;

        public GetSubscriptionsCommand(ITelegramBotService telegramBotService, IDB db) :base(telegramBotService) 
        {
            _db = db;
        }

        public override async Task ExecuteAsync(Update update)
        {
            var user = update.CallbackQuery?.From.Id;
            
            if (user != null)
            {
                var parameter = update.CallbackQuery?.Data?.Substring(update.CallbackQuery.Data.IndexOf("=") + 1);

                if (parameter != null)
                {
                    SubscriptionType sub = (SubscriptionType)int.Parse(parameter);

                    if (_db.HaveSubscription(user ?? 0, sub))
                    {
                        _db.RemoveSubscription(user ?? 0, sub);
                    }
                    else
                        _db.AddSubscription(user ?? 0, sub);
                }


                await _telegramBotService.ReplyAsync(update, "Preparing Files for Download!");
                await _telegramBotService.ReplyAsync(update, "Download Started.");
                return;
            }


            var inlineKeyboardMarkup = new InlineKeyboardMarkup
               (
                   new[]
                       {
                            new [] {  InlineKeyboardButton.WithCallbackData(SubscriptionType.CoinCapMarket.ToString(), "/subs=" + SubscriptionType.CoinCapMarket) },
                            new [] {  InlineKeyboardButton.WithCallbackData(SubscriptionType.XKCD.ToString(), "/subs=" + SubscriptionType.XKCD) },
                            new [] {  InlineKeyboardButton.WithCallbackData(SubscriptionType.Oglaf.ToString(), "/subs=" + SubscriptionType.Oglaf) },
                       }
               );
            await _telegramBotService.SendTextMessageWithButtonsAsync(update, "Select Subscription Type:", inlineKeyboardMarkup);
        }

        
    }
}
