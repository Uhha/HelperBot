using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace Logic
{
    public static class Helper
    {
        public static string ExtractCommand(Update update, bool isCallback = false)
        {
            var commandText = (isCallback) ? update.CallbackQuery.Data : update.Message.Text; 
            commandText = commandText.Replace(' ', '=');
            commandText = (commandText.Contains('=')) ? commandText.Substring(0, commandText.IndexOf('=')) : commandText;
            return commandText;
        }

        public static string DecimalToString(decimal dec)
        {
            string strdec = dec.ToString(CultureInfo.InvariantCulture);
            return strdec.Contains(".") ? strdec.TrimEnd('0').TrimEnd('.') : strdec;
        }

        public static string DecimalToString(decimal? dec)
        {
            if (dec == null) return "";
            return DecimalToString((decimal)dec);
        }
    }
}
