using DatabaseInteractions;
using NLog;
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

namespace Logic.Modules
{
    class TrendModule : IModule
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();

        public async Task GenerateAndSendAsync(TelegramBotClient bot, Update update)
        {
            
        }

        public Task GenerateAndSendCallbackAsync(TelegramBotClient bot, Update update)
        {
            throw new NotImplementedException();
        }

        public Task GenerateAndSendWorkerAsync(TelegramBotClient bot, IList<string> parameters = null)
        {
            throw new NotImplementedException();
        }

        public async void RecordCoinPrice()
        {
            HttpClient client = new HttpClient();
            int currenciesNumber = 5;
            var response = await client.GetAsync(string.Format("https://api.coinmarketcap.com/v1/ticker?limit={0}", currenciesNumber));
            if (response.IsSuccessStatusCode)
            {
                var result = response.Content.ReadAsAsync<CoinPrice[]>().Result;
                using (AlcoDBEntities db = new AlcoDBEntities())
                {
                    foreach (var item in result)
                    {
                        decimal.TryParse(item.price_usd, out decimal price);
                        db.CoinPriceRecords.Add(new CoinPriceRecords() { dtRecorded = DateTime.UtcNow , CoinSymbol = item.symbol, Price = price });
                    }
                    db.SaveChanges();
                }

            }

        }

    }
}
