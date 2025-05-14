using BotApi.Interfaces;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace BotApi.Commands
{
    public class QBDownloadTorrentCallbackCommand : BaseCommandAsync
    {
        private readonly IQBitService _qBitService;
        private readonly IQBUrlResolverService _qBUrlResolverService;

        public QBDownloadTorrentCallbackCommand(ITelegramBotService telegramBotService, IQBitService qBitService, IQBUrlResolverService qBUrlResolverService) :base(telegramBotService) 
        {
            _qBitService = qBitService;
            _qBUrlResolverService = qBUrlResolverService;
        }

        public override async Task ExecuteAsync(Update update)
        {
            var user = update.CallbackQuery.From.Id;
            var parameters = update.CallbackQuery.Data.Substring(update.CallbackQuery.Data.IndexOf("=") + 1);
            if (parameters.Contains("$p"))
            {
                var p = parameters.Split("$p");
                var uri = _qBUrlResolverService.GetUrl(p[0]);
                await _telegramBotService.ReplyAsync(update, "Preparing Files for Download!");
                await _qBitService.AddTorrentAsync(uri.ToString(), p[1], user);
                await _telegramBotService.ReplyAsync(update, "Download Started.");
                return;
            }

            var inlineKeyboardMarkup = new InlineKeyboardMarkup
                (
                    new[]
                        {
                            new [] {  InlineKeyboardButton.WithCallbackData("Movies", "/qdc=" + parameters + $"$p{foldersPaths[DownloadFolders.Movies]}") },
                            new [] {  InlineKeyboardButton.WithCallbackData("TVShows", "/qdc=" + parameters + $"$p{foldersPaths[DownloadFolders.TVShows]}") },
                            new [] {  InlineKeyboardButton.WithCallbackData("Anime", "/qdc=" + parameters + $"$p{foldersPaths[DownloadFolders.Anime]}") },
                            new [] {  InlineKeyboardButton.WithCallbackData("Music", "/qdc=" + parameters + $"$p{foldersPaths[DownloadFolders.Music]}") }
                        }
                );
            await _telegramBotService.SendTextMessageWithButtonsAsync(update, "Select Media Type:", inlineKeyboardMarkup);
        }

        private enum DownloadFolders
        {
            Movies,
            TVShows,
            Anime,
            Music
        }

        private readonly Dictionary<DownloadFolders, string> foldersPaths = new Dictionary<DownloadFolders, string>()
        {
            { DownloadFolders.Movies, "Movies" },
            { DownloadFolders.TVShows, "TVShows" },
            { DownloadFolders.Anime, "Anime" },
            { DownloadFolders.Music, "Music" },
        };
    }
}
