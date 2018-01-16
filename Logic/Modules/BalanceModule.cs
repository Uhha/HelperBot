using DatabaseInteractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Logic.Modules
{
    class BalanceModule : IModule
    {
        private Dictionary<String, float> _prices;

        public async Task GenerateAndSendAsync(TelegramBotClient bot, Update update)
        {
            _prices = new Dictionary<string, float>();
            double? totalAmount = 0;
            double? BTCtotalAmount = 0;
            double? ALTtotalAmount = 0;
            using (AlcoDBEntities db = new AlcoDBEntities())
            {
                var balances = db.Balances.Where(o => o.Client == (int)update.Message.Chat.Id);

                HttpClient client = new HttpClient();
                var response = await client.GetAsync(string.Format("https://api.coinmarketcap.com/v1/ticker/"));

                if (response.IsSuccessStatusCode)
                {
                    var result = response.Content.ReadAsAsync<CoinPrice[]>().Result;
                    foreach (var item in result)
                    {
                        float.TryParse(item.price_usd, out float price);
                        _prices.Add(item.symbol, price);
                    }
                }

                
                foreach (var item in balances)
                {
                    _prices.TryGetValue(item.Symbol, out float price);
                    totalAmount += (item.Shares * price);
                    if (item.Symbol == "BTC") BTCtotalAmount += (item.Shares * price);
                    if (item.Symbol != "BTC") ALTtotalAmount += (item.Shares * price);
                }
            }
            string msg = $"BTC = ${BTCtotalAmount}{Environment.NewLine}ALT = ${ALTtotalAmount}{Environment.NewLine}TOTAL = ${totalAmount}";
            await bot.SendTextMessageAsync(update.Message.Chat.Id, msg, parseMode: ParseMode.Html);
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
