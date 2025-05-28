using BotApi.Database;
using BotApi.Interfaces;
using BotApi.Services;
using Microsoft.Extensions.Options;
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
        private readonly ClientReplyStateService _clientReplyStateService;
        private readonly IDB _db;
        private DiscoveryApi _discoveryApi;

        public GetLiveConcertsCommand(ITelegramBotService telegramBotService, 
            IOptions<APIConfig> apiConfig,
            ILogger<GetLiveConcertsCommand> logger,
            ClientReplyStateService clientReplyStateService,
            IDB db

            ) :base(telegramBotService) 
        {
            _logger = logger;
            _apiConfig = apiConfig;
            _clientReplyStateService = clientReplyStateService;
            _db = db;
            _discoveryApi = new DiscoveryApi(_apiConfig.Value.TicketmasterApiKey);
        }

        public override async Task ExecuteAsync(Update update)
        {
            try
            {
                if (update.Message?.Chat?.Id is { } chatId)
                {
                    switch (_clientReplyStateService.GetExpectedReply(chatId)) 
                    {
                        case ExpectedReplyType.None:
                            break;

                        case ExpectedReplyType.BandSearch:
                            await SearchBandButtonsAsync(update, update.Message.Text);
                            //await _telegramBotService.ReplyAsync(update, string.Join("\n\n", message));
                            break;

                        case ExpectedReplyType.RemoveBand:

                            var bandIdToRemove = update.Message.Text;
                            if (bandIdToRemove == null)
                                break;

                            _db.RemoveBand(chatId, bandIdToRemove);
                            await _telegramBotService.ReplyAsync(update, "Band removed.");
                            break;
                    }
                    _clientReplyStateService.ClearExpectedReply(chatId);
                }

                var user = update.CallbackQuery?.From.Id;

                if (user != null)
                {
                    var parameter = update.CallbackQuery?.Data?.Substring(update.CallbackQuery.Data.IndexOf("=") + 1);
                    
                    if (parameter != null)
                    {
                        var callbackChatId = update.CallbackQuery?.Message?.Chat.Id ?? 0;

                        var parameters = parameter.Split('|');

                        LiveConcertsActionType cn = (LiveConcertsActionType)int.Parse(parameters[0]);

                        if (cn == LiveConcertsActionType.UpcomingConcerts)
                        {
                            var bands = _db.GetBands(callbackChatId);
                            var concerts = await GetUpcomingConcertsAsync(bands.Select(x => x.BandId).ToList());
                            var message = FormatTelegramMessage(concerts);

                            await _telegramBotService.ReplyAsync(update, message);
                        }

                        if (cn == LiveConcertsActionType.SearchBand)
                        {
                            await _telegramBotService.ReplyAsync(update, "Enter Band Name:");
                            _clientReplyStateService.SetExpectedReply(callbackChatId, ExpectedReplyType.BandSearch);
                        }

                        if (cn == LiveConcertsActionType.AddBand)
                        {
                            _db.AddBand(callbackChatId, parameters[1], parameters[2], parameters[3]);

                            await _telegramBotService.ReplyAsync(update, $"Band {parameters[1]}, {parameters[2]}, {parameters[3]} Added");
                            
                            
                            //_clientReplyStateService.SetExpectedReply(callbackChatId, ExpectedReplyType.AddBand);
                        }

                        if (cn == LiveConcertsActionType.RemoveBand)
                        {
                            await _telegramBotService.ReplyAsync(update, "Enter Band ID to Remove:");
                            _clientReplyStateService.SetExpectedReply(callbackChatId, ExpectedReplyType.RemoveBand);
                        }

                        if (cn == LiveConcertsActionType.TrackedBands)
                        {
                            var bands = _db.GetBands(callbackChatId);
                            await _telegramBotService.ReplyAsync(update, string.Join("\n", bands.Select(b => $"{b.BandName} ({b.Genre})")));
                        }
                    }
                    return;
                }


                var inlineKeyboardMarkup = new InlineKeyboardMarkup
                   (
                       new[]
                           {
                            new [] {  InlineKeyboardButton.WithCallbackData("Upcoming Concerts", "/live=" + (int)LiveConcertsActionType.UpcomingConcerts) },
                            new [] {  InlineKeyboardButton.WithCallbackData("Search Band", "/live=" + (int)LiveConcertsActionType.SearchBand) },
                            new [] {  InlineKeyboardButton.WithCallbackData("Add Band", "/live=" + (int)LiveConcertsActionType.AddBand) },
                            new [] {  InlineKeyboardButton.WithCallbackData("Remove Band", "/live=" + (int)LiveConcertsActionType.RemoveBand) },
                            new [] {  InlineKeyboardButton.WithCallbackData("Tracked Bands", "/live=" + (int)LiveConcertsActionType.TrackedBands) },
                            //new [] {  InlineKeyboardButton.WithCallbackData("Change Area", "/live=" + (int)LiveConcertsActionType.ChangeArea) },
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

        public async Task<List<string>> GetUpcomingConcertsAsync(List<string> artistIds, string city = "New York")
        {
            var results = new List<string>();

            foreach (var artistId in artistIds)
            {
                var searchRequest = new SearchEventsRequest()
                    .AddQueryParameter("AttractionId", artistId)
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

                            results.Add($"{artistId}: {name} on {date} at {venue}\n");
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

        public async Task SearchBandButtonsAsync(Update update, string? keyword)
        {
            if (string.IsNullOrEmpty(keyword))
                return;

            var results = new List<string>();

            var request = new SearchAttractionsRequest()
                .AddQueryParameter("Keyword", keyword)
                .AddQueryParameter("ClassificationName", "music")
                .AddQueryParameter("CountryCode", "US")
                .AddQueryParameter("Size", 10);


            var response = await _discoveryApi.Attractions.Search(request);

            var buttons = new InlineKeyboardMarkup();
           
            if (response?.Embedded?.Attractions != null)
            {
                foreach (var artist in response.Embedded.Attractions)
                {
                    var id = artist.Id;
                    var name = artist.Name;
                    var genre = artist.Classifications?[0]?.Genre?.Name;
                    var data = $"{(int)LiveConcertsActionType.AddBand}|{id}|{name}|{genre ?? "Unknown"}";
                    buttons.AddButton(new InlineKeyboardButton($"{name} ({genre ?? "Unknown Genre"})", "/live=" + data));
                    buttons.AddNewRow();
                    
                }
            }
            await _telegramBotService.SendTextMessageWithButtonsAsync(update, "Add band:", buttons);

        }

        public async Task<List<string>> SearchBandAsync(string? keyword)
        {
            if (string.IsNullOrEmpty(keyword))
                return new List<string>();

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
            SearchBand,
            AddBand,
            RemoveBand,
            TrackedBands,
            ChangeArea
        }
    }
}
