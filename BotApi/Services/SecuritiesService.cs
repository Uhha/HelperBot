using BotApi.Commands;
using BotApi.Database;
using BotApi.Interfaces;
using System.Text.RegularExpressions;
using Telegram.Bot.Types;

namespace BotApi.Services
{
    public class SecuritiesService : ISecuritiesService
    {
        private readonly ILogger<SecuritiesService> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IDB _db;

        public SecuritiesService(ILogger<SecuritiesService> logger, IHttpClientFactory httpClientFactory, IDB db)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _db = db;
        }

        public async Task<IList<string>> GetPricesAsync(long chatId)
        {
            IList<string> symbols = _db.GetSecurities(chatId);
            IList<string> prices = new List<string>();
            _logger.LogInformation(nameof(SecuritiesService) + $" Started collecting prices for {symbols.Count} symbols.");
            foreach (string symbol in symbols)
            {
                var price = await GetPrice(symbol);
                _logger.LogInformation(nameof(SecuritiesService) + $" Got price for {symbol} : {price}.");
                prices.Add(price);
            }
            _logger.LogInformation(nameof(SecuritiesService) + $" Collected total of {prices.Count} prices.");
            return prices;
        }

        private async Task<string> GetPrice(string symbol)
        {
            try
            {
                string apiUrl = $"https://finance.yahoo.com/quote/{symbol}";

                using (HttpClient httpClient = _httpClientFactory.CreateClient())
                {
                    HttpResponseMessage response = await httpClient.GetAsync(apiUrl);

                    if (response.IsSuccessStatusCode)
                    {
                        string htmlContent = await response.Content.ReadAsStringAsync();

                        string price = ExtractPriceFromHtml(htmlContent);
                        string changePercent = ExtractChangePercentFromHtml(htmlContent);

                        if (price != null && changePercent != null)
                        {
                            string result = $"{symbol} ${price} ({changePercent})";
                            return (result);
                        }
                        else
                        {
                            return "";
                        }
                    }
                    else
                    {
                        _logger.LogError($"Failed to get prices. Status code: {response.StatusCode}");
                        return "";
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occurred: {ex.Message}");
                return "";
            }
        }

        private string ExtractPriceFromHtml(string htmlContent)
        {
            try
            {
                // Check for the existence of the fin-streamer tag with data-test="qsp-price" attribute
                string pattern = @"<fin-streamer[^>]+data-test=""qsp-price""[^>]+value=""([^""]+)""[^>]*>([\d.]+)<\/fin-streamer>";
                Match match = Regex.Match(htmlContent, pattern);

                return match.Success ? match.Groups[2].Value : null;
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occurred while extracting price: {ex.Message}");
                return null;
            }
        }

        private string ExtractChangePercentFromHtml(string htmlContent)
        {
            try
            {
                string pattern = @"value=""([\d.-]+)"".*<span class=""C\(\$[^""]+\)"">([+/-]\d+\.\d+%?)</span>";
                Match match = Regex.Match(htmlContent, pattern);

                return match.Success ? match.Groups[2].Value : null;
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occurred while extracting change percent: {ex.Message}");
                return null;
            }
        }

        public bool AddSecurity(long chatId, string symbol)
        {
            return _db.AddSecurity(chatId, symbol);
        }

        public bool RemoveSecurity(long chatId, string symbol)
        {
            return _db.RemoveSecurity(chatId, symbol);
        }

        public IList<string> ListSecurities(long chatId)
        {
            return _db.GetSecurities(chatId);
        }
    }

    

}
