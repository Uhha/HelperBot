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
                var clients = DB.GetList<int>("select distinct c.chatId from Clients c " +
                        "join Subscriptions s on s.id = c.subscription " +
                        "where s.SubsctiptionType = " + (int)Subscription.ErrorMessageLog);

                foreach (var client in clients)
                {
                    await bot.SendTextMessageAsync(client, errormsg, ParseMode.Default);
                }
            }
            catch { }
        }
    }


}
