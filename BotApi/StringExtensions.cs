using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotApi
{
    public static class StringExtensions
    {
        public static string FirstCap(this string value)
        {
            if (String.IsNullOrEmpty(value))
                return "";
            return value.First().ToString().ToUpper() + value.Substring(1);
        }
        public static string Bold(this string value)
        {
            return "<b>" + value + "</b>";
        }
        public static string Italic(this string value)
        {
            return "<i>" + value + "</i>";
        }

    }
}
