﻿using BotApi.Commands;
using BotApi.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using QBittorrent.Client;
using System.Net;
using System.Text;

namespace BotApi.Services
{
    public class GetCoinsService: IGetCoinsService
    {
        private readonly IOptions<APIConfig> _apiConfig;
        private readonly ILogger<GetCoinsService> _logger;

        public GetCoinsService(IOptions<APIConfig> apiConfig, ILogger<GetCoinsService> logger)
        {
            _apiConfig = apiConfig;
            _logger = logger;
        }

        public async Task<(string, bool)> GetPricesAsync(int currenciesNumber)
        {
            //HttpClient client = HttpClientProvider.GetClient();
            if (currenciesNumber > 50) currenciesNumber = 50;
            if (currenciesNumber < 0) return ("Incorrect number", true);
            if (currenciesNumber == 0) currenciesNumber = 5;
            try
            {

                var URL = new UriBuilder("https://pro-api.coinmarketcap.com/v1/cryptocurrency/listings/latest");
                //var queryString = HttpUtility.ParseQueryString(string.Empty);
                //queryString["id"] = "1,2,3";

                //URL.Query = queryString.ToString();

                var client = new WebClient();
                client.Headers.Add("X-CMC_PRO_API_KEY", _apiConfig.Value.CoinMarketCapAPIKey);
                client.Headers.Add("Accepts", "application/json");
                var response = client.DownloadString(URL.ToString());

                response = response.Replace("USD", "CurrentPriceInfo");

                CoinmarketcapItemData result = JsonConvert.DeserializeObject<CoinmarketcapItemData>(response);
                List<Currency> currencyList = new List<Currency>();

                foreach (ItemData data in result.DataList)
                {
                    if (data.symbol.Contains("CurrentPriceInfo")) continue;
                    Currency item = new Currency
                    {
                        Id = data.id.ToString(),
                        Name = data.name,
                        Symbol = data.symbol,
                        Rank = data.cmc_rank.ToString(),
                        Price = data.quote.CurrentPriceInfo.price ?? 0d,
                        Volume24hUsd = data.quote.CurrentPriceInfo.volume_24h ?? 0,
                        MarketCapUsd = data.quote.CurrentPriceInfo.volume_24h ?? 0,
                        PercentChange1h = data.quote.CurrentPriceInfo.percent_change_1h ?? 0,
                        PercentChange24h = data.quote.CurrentPriceInfo.percent_change_24h ?? 0,
                        PercentChange7d = data.quote.CurrentPriceInfo.percent_change_7d ?? 0,
                        LastUpdated = data.quote.CurrentPriceInfo.last_updated,
                        MarketCapConvert = data.quote.CurrentPriceInfo.market_cap ?? 0d,
                        ConvertCurrency = "USD"
                    };

                    currencyList.Add(item);
                }


                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < currenciesNumber; i++)
                {
                    sb.Append($"{currencyList[i].Symbol.Bold()}:  ${Math.Round(currencyList[i].Price, 2).ToString()},  {Math.Round(currencyList[i].PercentChange24h, 1).ToString().Italic()}%{Environment.NewLine}");
                }

                return (sb.ToString(), false);

            }
            catch (Exception)
            {
                return ("", false);
            }
        }

    }

    public class Currency
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Symbol { get; set; }
        public string Rank { get; set; }
        public double Price { get; set; }
        public double Volume24hUsd { get; set; }
        public double MarketCapUsd { get; set; }
        public double PercentChange1h { get; set; }
        public double PercentChange24h { get; set; }
        public double PercentChange7d { get; set; }
        public DateTime LastUpdated { get; set; }
        public Double MarketCapConvert { get; set; }
        public string ConvertCurrency { get; set; }
    }

    public class CoinmarketcapItemData
    {
        public Status Status { get; set; }

        public CoinmarketcapItemData()
        {
            DataList = new List<ItemData>();
        }

        [JsonProperty(PropertyName = "data")]
        public List<ItemData> DataList { get; set; }

        private Dictionary<string, ItemData> _dataItemList;
        [JsonProperty(PropertyName = "dataItem")]
        public Dictionary<string, ItemData> DataItemList
        {
            get { return _dataItemList; }
            set
            {
                _dataItemList = value;
                DataList = value.Values.ToList();
            }
        }
    }

    public class ItemData
    {
        public int id { get; set; }
        public string name { get; set; }
        public string symbol { get; set; }
        public string slug { get; set; }
        public int num_market_pairs { get; set; }
        public DateTime date_added { get; set; }
        public List<object> tags { get; set; }
        public double? max_supply { get; set; }
        public double? circulating_supply { get; set; }
        public double? total_supply { get; set; }
        public CurrentInfo platform { get; set; }
        public int cmc_rank { get; set; }
        public DateTime last_updated { get; set; }
        public Quote quote { get; set; }
    }

    public class Status
    {
        public DateTime timestamp { get; set; }
        public int error_code { get; set; }
        public object error_message { get; set; }
        public int elapsed { get; set; }
        public int credit_count { get; set; }
        public object notice { get; set; }
    }

    public class CurrentInfo
    {
        public int id { get; set; }
        public string name { get; set; }
        public string symbol { get; set; }
        public string slug { get; set; }
        public string token_address { get; set; }
    }

    public class CurrentPriceInfo
    {
        public double? price { get; set; }
        public double? volume_24h { get; set; }
        public double? percent_change_1h { get; set; }
        public double? percent_change_24h { get; set; }
        public double? percent_change_7d { get; set; }
        public double? market_cap { get; set; }
        public DateTime last_updated { get; set; }
    }

    public class Quote
    {
        public CurrentPriceInfo CurrentPriceInfo { get; set; }
    }

}
