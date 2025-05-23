using BotApi.Database;
using BotApi.Interfaces;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Web;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace BotApi.Commands
{
    public class GetLiveConcertsCommand : BaseCommandAsync
    {
        private ILogger<GetLiveConcertsCommand> _logger;
        private IOptions<APIConfig> _apiConfig;
        private IHttpClientFactory _httpsClientFactory;
        private const string TicketmasterBaseUrl = "https://app.ticketmaster.com/discovery/v2/events.json";

        public GetLiveConcertsCommand(ITelegramBotService telegramBotService, 
            IOptions<APIConfig> apiConfig,
            IHttpClientFactory httpClientFactory,
            ILogger<GetLiveConcertsCommand> logger

            ) :base(telegramBotService) 
        {
            _logger = logger;
            _apiConfig = apiConfig;
            _httpsClientFactory = httpClientFactory;
        }

        public override async Task ExecuteAsync(Update update)
        {
            try
            {
                var user = update.CallbackQuery?.From.Id;

                if (user != null)
                {
                    var parameter = update.CallbackQuery?.Data?.Substring(update.CallbackQuery.Data.IndexOf("=") + 1);

                    if (parameter != null)
                    {
                        LiveConcertsActionType cn = (LiveConcertsActionType)int.Parse(parameter);

                        if (cn == LiveConcertsActionType.UpcomingConcerts)
                        {
                            var concerts = await GetUpcomingConcertsAsync(["Lamb of god", "Ghost"]);
                            var message = FormatTelegramMessage(concerts);

                            await _telegramBotService.ReplyAsync(update, message);
                        }
                        
                    }
                    return;
                }


                var inlineKeyboardMarkup = new InlineKeyboardMarkup
                   (
                       new[]
                           {
                            new [] {  InlineKeyboardButton.WithCallbackData(LiveConcertsActionType.UpcomingConcerts.ToString(), "/live=" + (int)LiveConcertsActionType.UpcomingConcerts) },
                            new [] {  InlineKeyboardButton.WithCallbackData(LiveConcertsActionType.AddBand.ToString(), "/live=" + (int)LiveConcertsActionType.AddBand) },
                            new [] {  InlineKeyboardButton.WithCallbackData(LiveConcertsActionType.RemoveBand.ToString(), "/live=" + (int)LiveConcertsActionType.RemoveBand) },
                            new [] {  InlineKeyboardButton.WithCallbackData(LiveConcertsActionType.ChangeArea.ToString(), "/live=" + (int)LiveConcertsActionType.ChangeArea) },
                           }
                   );
                await _telegramBotService.SendTextMessageWithButtonsAsync(update, "Select Option:", inlineKeyboardMarkup);
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                throw;
            }
        }

        public async Task<List<string>> GetUpcomingConcertsAsync(List<string> artistNames, string city = "New York")
        {
            var foundConcerts = new List<string>();

            foreach (var artist in artistNames)
            {
                var query = HttpUtility.ParseQueryString(string.Empty);
                query["apikey"] = _apiConfig.Value.TicketmasterApiKey;
                query["keyword"] = artist;
                query["city"] = city;
                query["countryCode"] = "US";
                query["radius"] = "125"; // miles
                query["unit"] = "miles";
                query["size"] = "5"; // limit to 5 events per artist

                var url = $"{TicketmasterBaseUrl}?{query}";

                var client = _httpsClientFactory.CreateClient();

                var response = await client.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                    continue;

                var content = await response.Content.ReadAsStringAsync();
                var json = JObject.Parse(content);

                var events = json["_embedded"]?["events"];
                if (events != null)
                {
                    foreach (var ev in events)
                    {
                        var name = ev["name"]?.ToString();
                        var date = ev["dates"]?["start"]?["localDate"]?.ToString();
                        var venue = ev["_embedded"]?["venues"]?[0]?["name"]?.ToString();
                        var urlLink = ev["url"]?.ToString();

                        var summary = $"{artist}: {name} on {date} at {venue}\n{urlLink}";
                        foundConcerts.Add(summary);
                    }
                }
            }

            return foundConcerts;
        }

        public string FormatTelegramMessage(List<string> concerts)
        {
            if (concerts.Count == 0)
                return "No upcoming concerts found.";

            return "🎵 *Upcoming Concerts in NYC:*\n\n" + string.Join("\n\n", concerts);
        }

        public enum LiveConcertsActionType
        {
            UpcomingConcerts,
            AddBand,
            RemoveBand,
            ChangeArea
        }
    }
}
