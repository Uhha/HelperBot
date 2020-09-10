using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Logic.Modules
{
    public class WakeOnLanModule 
    {
        public async Task GenerateAndSendAsync(TelegramBotClient bot, Update update)
        {
            var text = update.Message.Text.Split(' ');
            switch (text.Count())
            {
                case 1:
                case 2:
                    await bot.SendTextMessageAsync(update.Message.Chat.Id, "Use like: /wol 1.2.3.4 01:02:03:04:05:06 7");
                    break;
                default:
                    if (!WakeOnLanModule.ValidateMac(text[2]))
                        await bot.SendTextMessageAsync(update.Message.Chat.Id, "Incorrect MAC address");
                    else
                    {
                        try
                        {
                            WakeOnLanModule.Up(text[1], text[2], GetPort(text));
                            await bot.SendTextMessageAsync(update.Message.Chat.Id, "Magic packet sent!");
                        }
                        catch (Exception e)
                        {
                            await bot.SendTextMessageAsync(update.Message.Chat.Id, "Error: " + e.Message);
                        }
                    }
                    break;
            }

            int? GetPort(IReadOnlyList<string> tx)
            {
                if (tx.Count == 4 && int.TryParse(tx[3], out int port))
                    return port;
                return null;
            }
        }

        public Task GenerateAndSendCallbackAsync(TelegramBotClient bot, Update update)
        {
            throw new NotImplementedException();
        }

        public Task GenerateAndSendWorkerAsync(TelegramBotClient bot, IList<string> parameters = null)
        {
            throw new NotImplementedException();
        }

        public static void Up(string ip, string mac, int? port = null)
        {
            var client = new UdpClient();
            var data = new byte[102];

            for (var i = 0; i <= 5; i++)
                data[i] = 0xff;

            var macDigits = GetMacDigits(mac);
            if (macDigits.Length != 6)
                throw new ArgumentException("Incorrect MAC address supplied!");

            const int start = 6;
            for (var i = 0; i < 16; i++) 
                for (var x = 0; x < 6; x++)
                    data[start + i * 6 + x] = (byte)Convert.ToInt32(macDigits[x], 16);

            client.Send(data, data.Length, ip, port ?? 7);
        }

        private static string[] GetMacDigits(string mac) 
        {
            return mac.Split(mac.Contains("-") ? '-' : ':');
        }

        public static bool ValidateMac(string mac) 
        {
            return GetMacDigits(mac).Length == 6;
        }

    
    }
}