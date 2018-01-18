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
        private Dictionary<String, double> _prices;

        public async Task GenerateAndSendAsync(TelegramBotClient bot, Update update)
        {
            _prices = new Dictionary<string, double>();
            double totalAmount = 0;
            double BTCtotalAmount = 0;
            double ALTtotalAmount = 0;
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
                        double.TryParse(item.price_usd, out double price);
                        _prices.Add(item.symbol, price);
                    }
                }

                
                foreach (var item in balances)
                {
                    _prices.TryGetValue(item.Symbol, out double price);
                    totalAmount += ((double)item.Shares * price);
                    if (item.Symbol == "BTC") BTCtotalAmount += ((double)item.Shares * price);
                    if (item.Symbol != "BTC") ALTtotalAmount += ((double)item.Shares * price);
                }
            }
            string msg = $"BTC: ${Math.Round(BTCtotalAmount, 2)}{Environment.NewLine}ALT: ${Math.Round(ALTtotalAmount, 2)}{Environment.NewLine}TOTAL: ${Math.Round(totalAmount,2)}";
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
            var parsed = double.TryParse(params1[2], out double value);
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
                    result.Shares = (decimal)value;
                    message = symbol + " Record Updated!";
                }
                else
                {
                    var balance = new Balances()
                    {
                        Client = client,
                        Symbol = symbol,
                        Shares = (decimal)value
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
            _prices = new Dictionary<string, double>();
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
                        double.TryParse(item.price_usd, out double price);
                        _prices.Add(item.symbol, price);
                    }
                }


                foreach (var item in balances)
                {
                    _prices.TryGetValue(item.Symbol, out double price);
                    message.Append($"{item.Symbol}:   {Helper.DecimalToString(item.Shares)}    ${Math.Round(price,2)}     ${Math.Round((double)item.Shares*price, 2).ToString().Bold()}{Environment.NewLine}");
                }
            }
            await bot.SendTextMessageAsync(update.Message.From.Id, message.ToString(), parseMode: ParseMode.Html);
        }
     
    }
}
