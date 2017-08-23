using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Logic.Modules
{
    public interface IModule
    {
        Task GenerateAndSendAsync(TelegramBotClient bot, Update update);
        Task GenerateAndSendCallbackAsync(TelegramBotClient bot, Update update);
        Task GenerateAndSendWorkerAsync(TelegramBotClient bot, IList<string> parameters = null);
    }
}
