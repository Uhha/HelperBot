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
        private IServiceScopeFactory _scopeFactory;
        private ILogger<GetSubscriptionsCommand> _logger;

        public GetSubscriptionsCommand(ITelegramBotService telegramBotService, 
            IServiceScopeFactory scopeFactory,
            ILogger<GetSubscriptionsCommand> logger
            ) :base(telegramBotService) 
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        public override async Task ExecuteAsync(Update update)
        {
            try
            {
                var user = update.CallbackQuery?.From.Id;

                if (user != null)
                {
                    var parameter = update.CallbackQuery?.Data?.Substring(update.CallbackQuery.Data.IndexOf("=") + 1);

                    if (parameter != null)
                    {
                        SubscriptionType sub = (SubscriptionType)int.Parse(parameter);

                        using (var scope = _scopeFactory.CreateScope())
                        {
                            var db = scope.ServiceProvider.GetRequiredService<IDB>();
                            if (db.HaveSubscription(user ?? 0, sub))
                            {
                                db.RemoveSubscription(user ?? 0, sub);
                                await _telegramBotService.ReplyAsync(update, $"Removed subscription to {sub}");
                            }
                            else
                            {
                                db.AddSubscription(user ?? 0, sub);
                                await _telegramBotService.ReplyAsync(update, $"Added subscription to {sub}");
                            } 
                        }
                    }
                    return;
                }


                var inlineKeyboardMarkup = new InlineKeyboardMarkup
                   (
                       new[]
                           {
                            new [] {  InlineKeyboardButton.WithCallbackData(SubscriptionType.CoinCapMarket.ToString(), "/subs=" + (int)SubscriptionType.CoinCapMarket) },
                            new [] {  InlineKeyboardButton.WithCallbackData(SubscriptionType.XKCD.ToString(), "/subs=" + (int)SubscriptionType.XKCD) },
                            new [] {  InlineKeyboardButton.WithCallbackData(SubscriptionType.Oglaf.ToString(), "/subs=" + (int)SubscriptionType.Oglaf) },
                            new [] {  InlineKeyboardButton.WithCallbackData(SubscriptionType.SecuritiesPrices.ToString(), "/subs=" + (int)SubscriptionType.SecuritiesPrices) },
                           }
                   );
                await _telegramBotService.SendTextMessageWithButtonsAsync(update, "Select Subscription Type:", inlineKeyboardMarkup);
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                throw;
            }
        }

        
    }
}
