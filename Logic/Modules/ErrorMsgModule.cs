using DatabaseInteractions;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Tracer;
using Web.Providers;

namespace Logic.Modules
{
    class ErrorMsgModule
    {
        public async Task SendErrorMessage(TelegramBotClient bot, string errormsg)
        {
            try
            {
                using (BotDBContext db = new BotDBContext())
                {
                    var clients = (from c in db.Clients
                                   join sub in db.Subscriptions on c.Subscription equals sub.Id
                                   where sub.SubsctiptionType == (int)Subscription.ErrorMessageLog
                                   select c.ChatId
                                   ).Distinct();

                    foreach (var client in clients)
                    {
                        await bot.SendTextMessageAsync(client, errormsg);
                    }
                }
            }
            catch { }
        }
    }


}
