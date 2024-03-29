﻿using DatabaseInteractions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Tracer;
using Web.Providers;

namespace Logic.Modules
{
    public class BalanceModule 
    {

        private static string API_KEY = Config.CoinMarketCapAPIKey;
        public async Task GenerateAndSendAsync(TelegramBotClient bot, Update update)
        {
            TraceError.Info($"In Balance module");
            double totalAmount = 0;
            double BTCtotalAmount = 0;
            double ALTtotalAmount = 0;
            try
            {
                using (BotDBContext db = new BotDBContext())
                {
                    TraceError.Info($"DB initiated");
                    var balances = db.Balances.Where(o => o.Client == (int)update.Message.From.Id);

                    StringBuilder sb = new StringBuilder();
                    foreach (var b in balances)
                    {
                        sb.Append(b.Symbol + ',');
                    }
                    sb.Remove(sb.Length - 1, 1);

                    var prices = GetPrices(sb.ToString());

                    foreach (var item in balances)
                    {
                        prices.TryGetValue(item.Symbol, out double price);
                        totalAmount += ((double)item.Shares * price);
                        if (item.Symbol == "BTC") BTCtotalAmount += ((double)item.Shares * price);
                        if (item.Symbol != "BTC") ALTtotalAmount += ((double)item.Shares * price);
                    }
                }
            }
            catch (Exception e)
            {
                TraceError.Info("EXception thrown ");
                TraceError.Info("DB ERROR: " + e.Message);
            }
            string msg = $"BTC: ${Math.Round(BTCtotalAmount, 2)}{Environment.NewLine}ALT: ${Math.Round(ALTtotalAmount, 2)}{Environment.NewLine}TOTAL: ${Math.Round(totalAmount, 2)}";
            await bot.SendTextMessageAsync(update.Message.From.Id, msg, parseMode: ParseMode.Html);
        }

        private Dictionary<string, double> GetPrices(string symbols)
        {
            var URL = new UriBuilder("https://pro-api.coinmarketcap.com/v1/cryptocurrency/quotes/latest");
            var queryString = HttpUtility.ParseQueryString(string.Empty);
            queryString["symbol"] = symbols;

            URL.Query = queryString.ToString();

            var client = new WebClient();
            client.Headers.Add("X-CMC_PRO_API_KEY", API_KEY);
            client.Headers.Add("Accepts", "application/json");
            var response = client.DownloadString(URL.ToString());
            response = response.Replace("USD", "CurrentPriceInfo");

            //dynamic result = JsonConvert.DeserializeObject(response);

            JObject obj = JObject.Parse(response);
            JEnumerable<JToken> data = obj["data"].Children();

            Dictionary<string, double> prices = new Dictionary<string, double>();
            foreach (var t in data)
            {
                var tokensvalue = t.Children();
                var tokenData = tokensvalue.First().ToString();
                var entry = JsonConvert.DeserializeObject<ItemData>(tokenData);
                prices.Add(entry.symbol, entry.quote?.CurrentPriceInfo?.price ?? 0);
            }
            return prices;
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
            using (var db = new BotDBContext())
            {
                var result = db.Balances.SingleOrDefault(o => o.Client == client && o.Symbol == symbol);
                if (result != null)
                {
                    result.Shares = (decimal)value;
                    message = symbol + " Record Updated!";
                }
                else
                {
                    var balance = new Balance()
                    {
                        //TODO: Client should be long; remove cast
                        Client = (int)client,
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
            catch (Exception e)
            {
                TraceError.Error(e);
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
            using (var db = new BotDBContext())
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
            catch (Exception e)
            {
                TraceError.Error(e);
            }
        }

        internal async Task BalanceDetailsAsync(TelegramBotClient bot, Update update)
        {
            StringBuilder message = new StringBuilder();
            using (BotDBContext db = new BotDBContext())
            {
                var balances = db.Balances.Where(o => o.Client == (int)update.Message.From.Id);
                StringBuilder sb = new StringBuilder();
                foreach (var b in balances)
                {
                    sb.Append(b.Symbol + ',');
                }
                sb.Remove(sb.Length - 1, 1);

                var prices = GetPrices(sb.ToString());

                foreach (var item in balances)
                {
                    prices.TryGetValue(item.Symbol, out double price);
                    message.Append($"{item.Symbol}: {Helper.DecimalToString(item.Shares)} | ${Math.Round(price, 2)} | ${Math.Round((double)item.Shares * price, 2).ToString().Bold()}{Environment.NewLine}");
                }
            }
            await bot.SendTextMessageAsync(update.Message.From.Id, message.ToString(), parseMode: ParseMode.Html);
        }

    }
}
