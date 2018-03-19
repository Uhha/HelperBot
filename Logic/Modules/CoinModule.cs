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
    class CoinModule : IModule
    {
        public async Task GenerateAndSendAsync(TelegramBotClient bot, Update update)
        {
            var number = update.Message.Text.Substring(update.Message.Text.IndexOf(' ') + 1);
            int.TryParse(number, out int currenciesNumber);
            var result = await GetPricesAsync(currenciesNumber);
            if (string.IsNullOrEmpty(result.Item1)) return;
            await bot.SendTextMessageAsync(update.Message.Chat.Id, result.Item1, parseMode: ParseMode.Html);
        }

        public Task GenerateAndSendCallbackAsync(TelegramBotClient bot, Update update)
        {
            throw new NotImplementedException();
        }

        public async Task GenerateAndSendWorkerAsync(TelegramBotClient bot, IList<string> parameters)
        {
            var currenciesNumber = 5;
            bool.TryParse(parameters[0], out bool sendAnyway);
            var result = await GetPricesAsync(currenciesNumber);
            if ((!sendAnyway && !result.Item2) || string.IsNullOrEmpty(result.Item1)) return;

            try
            {
                var clients = DB.GetList<int>("select distinct c.chatId from Clients c " +
                        "join Subscriptions s on s.id = c.subscription " +
                        "where s.SubsctiptionType = " + (int)Subscription.CoinCapMarket);

                foreach (var client in clients)
                {
                    await bot.SendTextMessageAsync(client, result.Item1, parseMode: Telegram.Bot.Types.Enums.ParseMode.Html);
                }
            }
            catch (Exception e)
            {
                TraceError.Error(e);
            }
        }

        private async static Task<(string, bool)> GetPricesAsync(int currenciesNumber)
        {
            HttpClient client = HttpClientProvider.GetClient();
            if (currenciesNumber > 20) currenciesNumber = 20;
            if (currenciesNumber < 0) return ("Incorrect number", true);
            if (currenciesNumber == 0) currenciesNumber = 5;
            try
            {
                var response = await client.GetAsync(string.Format("https://api.coinmarketcap.com/v1/ticker?limit={0}", currenciesNumber));

                if (response.IsSuccessStatusCode)
                {
                    var result = response.Content.ReadAsAsync<CoinPrice[]>().Result;
                    StringBuilder sb = new StringBuilder();
                    foreach (var item in result)
                    {
                        sb.Append($"{item.symbol.Bold()}: ${item.price_usd}, {item.percent_change_24h.Italic()}%{Environment.NewLine}");
                    }

                    if (currenciesNumber == 1)
                    {
                        return (sb.ToString(), true);
                    }
                    else
                    {
                        double.TryParse(result[0].percent_change_24h, out double BTCchange);
                        double.TryParse(result[1].percent_change_24h, out double ETHchange);
                        var change = (Math.Abs(BTCchange) >= 10 || Math.Abs(ETHchange) >= 10) ? true : false;
                        return (sb.ToString(), change);
                    }
                }
                return (string.Empty, false);
            }
            catch (Exception e)
            {
                TraceError.Error(e);
                return ("", false);
            }
        }
    }

    internal class CoinPrice
    {
        public string id { get; set; }
        public string name { get; set; }
        public string symbol { get; set; }
        public string rank { get; set; }
        public string price_usd { get; set; }
        public string price_btc { get; set; }
        public string _24h_volume_usd { get; set; }
        public string market_cap_usd { get; set; }
        public string available_supply { get; set; }
        public string total_supply { get; set; }
        public string percent_change_1h { get; set; }
        public string percent_change_24h { get; set; }
        public string percent_change_7d { get; set; }
        public string last_updated { get; set; }
    }


}
