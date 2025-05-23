using BotApi.Database;
using BotApi.Interfaces;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using System;
using System.Web;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using Ticketmaster.Discovery;
using Ticketmaster.Discovery.Models;

namespace BotApi.Commands
{
    public class GetLiveConcertsCommand : BaseCommandAsync
    {
        private ILogger<GetLiveConcertsCommand> _logger;
        private IOptions<APIConfig> _apiConfig;
        private IHttpClientFactory _httpsClientFactory;
        private const string TicketmasterBaseUrl = "https://app.ticketmaster.com/discovery/v2/events.json";
        private DiscoveryApi _discoveryApi;

        public GetLiveConcertsCommand(ITelegramBotService telegramBotService, 
            IOptions<APIConfig> apiConfig,
            IHttpClientFactory httpClientFactory,
            ILogger<GetLiveConcertsCommand> logger

            ) :base(telegramBotService) 
        {
            _logger = logger;
            _apiConfig = apiConfig;
            _httpsClientFactory = httpClientFactory;

            _discoveryApi = new DiscoveryApi(_apiConfig.Value.TicketmasterApiKey);

            

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

                        if (cn == LiveConcertsActionType.AddBand)
                        {
                            var message = await SearchBandAsync("Ghost");
                            await _telegramBotService.ReplyAsync(update, string.Join("\n\n", message));
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
            var results = new List<string>();

            foreach (var artist in artistNames)
            {
                var searchRequest = new SearchEventsRequest()
                    .AddQueryParameter("Keyword", artist)
                    .AddQueryParameter("City", city)
                    .AddQueryParameter("ClassificationName", "music")
                    .AddQueryParameter("CountryCode", "US")
                    .AddQueryParameter("Size", 5);

                try
                {
                    var response = await _discoveryApi.Events.Search(searchRequest);

                    if (response?.Embedded?.Events != null)
                    {
                        foreach (var ev in response.Embedded.Events)
                        {
                            var name = ev.Name;
                            var date = ev.Dates?.Start?.LocalDate;
                            var venue = ev.Embedded?.Venues?[0]?.Name;
                            //var url = ev.Url;

                            results.Add($"{artist}: {name} on {date} at {venue}\n");
                        }
                    }
                }
                catch (Exception e)
                {
                    _logger.LogError("Load events failed", e);
                    throw;
                }
            }

            return results;
        }

        public string FormatTelegramMessage(List<string> concerts)
        {
            if (concerts.Count == 0)
                return "No upcoming concerts found.";

            return "🎵 *Upcoming Concerts in NYC:*\n\n" + string.Join("\n\n", concerts);
        }

        public async Task<List<string>> SearchBandAsync(string keyword)
        {
            var results = new List<string>();

            var request = new SearchAttractionsRequest()
                .AddQueryParameter("Keyword", keyword)
                .AddQueryParameter("ClassificationName", "music")
                .AddQueryParameter("CountryCode", "US")
                .AddQueryParameter("Size", 10);
            

            var response = await _discoveryApi.Attractions.Search(request);

            if (response?.Embedded?.Attractions != null)
            {
                foreach (var artist in response.Embedded.Attractions)
                {
                    var name = artist.Name;
                    var id = artist.Id;
                    var genre = artist.Classifications?[0]?.Genre?.Name;
                    var segment = artist.Classifications?[0]?.Segment?.Name;

                    results.Add($"{name} ({genre ?? "Unknown Genre"}) [{id}]");
                }
            }

            return results;
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
