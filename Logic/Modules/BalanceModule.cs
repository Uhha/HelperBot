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
using Telegram.Bot.Types.InlineKeyboardButtons;
using Telegram.Bot.Types.ReplyMarkups;

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
                var balances = db.Balances.Where(o => o.Client == (int)update.Message.From.Id);

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
            string msg = $"BTC: ${BTCtotalAmount}{Environment.NewLine}ALT: ${ALTtotalAmount}{Environment.NewLine}TOTAL: ${totalAmount}";
            await bot.SendTextMessageAsync(update.Message.From.Id, msg, parseMode: ParseMode.Html);
        }


        public Task GenerateAndSendCallbackAsync(TelegramBotClient bot, Update update)
        {
            throw new NotImplementedException();
        }

        public Task GenerateAndSendWorkerAsync(TelegramBotClient bot, IList<string> parameters = null)
        {
            throw new NotImplementedException();
        }

        internal async Task BalanceAddAsync(TelegramBotClient bot, Update update)
        {
            var params1 = update.Message.Text.Split(' ');
            if (params1.Length != 3)
            {
                await bot.SendTextMessageAsync(update.Message.From.Id, "Incorrect parameters", parseMode: ParseMode.Html);
                return;
            }

            var symbol = params1[1].ToUpper();
            var parsed = float.TryParse(params1[2], out float value);
            var client = update.Message.From.Id;
            if (string.IsNullOrEmpty(symbol) && parsed)
            {
                await bot.SendTextMessageAsync(update.Message.From.Id, "Incorrect parameters", parseMode: ParseMode.Html);
            }

            string message;
            using (var db = new AlcoDBEntities())
            {
                var result = db.Balances.SingleOrDefault(o => o.Client == client && o.Symbol == symbol);
                if (result != null)
                {
                    result.Shares = value;
                    message = symbol + " Record Updated!";
                }
                else
                {
                    var balance = new Balances()
                    {
                        Client = client,
                        Symbol = symbol,
                        Shares = value
                    };
                    db.Balances.Add(balance);
                    message = symbol + " Record Added!";
                }
                db.SaveChanges();
            }  

            try
            {
                await bot.SendTextMessageAsync(update.Message.From.Id, message);
            }
            catch (Exception)
            {
            }
        }


        internal async Task BalanceRemoveAsync(TelegramBotClient bot, Update update)
        {
            var params1 = update.Message.Text.Split(' ');
            var client = update.Message.From.Id;
            if (params1.Length != 2 && string.IsNullOrEmpty(params1[1]))
            {
                await bot.SendTextMessageAsync(update.Message.From.Id, "Incorrect parameters");
                return;
            }
            var symbol = params1[1].ToUpper();

            string message;
            using (var db = new AlcoDBEntities())
            {
                var result = db.Balances.SingleOrDefault(o => o.Client == client && o.Symbol == symbol);
                if (result != null)
                {
                    db.Balances.Remove(result);
                    message = symbol + " Record Deleted!";
                }
                else
                {
                    message = symbol + " Record Not Found!";
                }
                db.SaveChanges();
            }

            try
            {
                await bot.SendTextMessageAsync(update.Message.From.Id, message);
            }
            catch (Exception)
            {
            }
        }

        internal async Task BalanceDetailsAsync(TelegramBotClient bot, Update update)
        {
            _prices = new Dictionary<string, float>();
            StringBuilder message = new StringBuilder();
            using (AlcoDBEntities db = new AlcoDBEntities())
            {
                var balances = db.Balances.Where(o => o.Client == (int)update.Message.From.Id);

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
                    message.Append($"{item.Symbol}: {item.Shares} ${price}{Environment.NewLine}");
                }
            }
            await bot.SendTextMessageAsync(update.Message.From.Id, message.ToString(), parseMode: ParseMode.Html);
        }
     
    }
}
