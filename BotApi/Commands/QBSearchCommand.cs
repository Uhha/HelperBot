using BotApi.Interfaces;
using BotApi.Services;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Microsoft.OpenApi.Services;
using Newtonsoft.Json;
using QBittorrent.Client;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO.Pipelines;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using static BotApi.Commands.RegisterCommands;

namespace BotApi.Commands
{
    public class QBSearchCommand : BaseCommandAsync
    {
        private readonly IQBitService _qBitService;
        private readonly IQBUrlResolverService _qBUrlResolverService;
        //private static readonly int MAX_MESSAGE_SIZE = 4096;
        private static readonly int MAX_NUMBER_OF_ENTRIES_TO_RETURN = 5;

        

        public QBSearchCommand(ITelegramBotService telegramBotService, IQBitService qBitService, IQBUrlResolverService qBUrlResolverService) :base(telegramBotService) 
        {
            _qBitService = qBitService;
            _qBUrlResolverService = qBUrlResolverService;
            
        }

        private TimeSpan _serachTimeout = TimeSpan.FromSeconds(20);

        public override async Task ExecuteAsync(Update update)
        {
            var searchText = update.Message?.Text.Substring(update.Message.Text.IndexOf(' ') + 1);
            
            if (searchText != null && searchText.Length > 0)
            {
                _qBUrlResolverService.Clear();

                var searchId = await _qBitService.StartSearchAsync(searchText);
                await _telegramBotService.ReplyAsync(update, "Search started.");

                var results = await PollForResults(searchId);

                if (results != null && results.Results.Count > 0)
                {
                    await SendSearchData(update, results, searchId);
                    return;
                }
            }

            var parameters = update.CallbackQuery?.Data?.Substring(update.CallbackQuery.Data.IndexOf("=") + 1);
            
            if (parameters != null && parameters.Length > 0)
            {
                if (parameters.Contains("$p"))
                {
                    var p = parameters.Split("$p");
                    if (p.Length != 2)
                        return;

                    int searchId = int.Parse(p[0]);
                    int skipFirst = int.Parse(p[1]);

                    var results = await PollForResults(searchId);

                    if (results != null && results.Results.Count > 0)
                    {
                        await SendSearchData(update, results, searchId, skipFirst: skipFirst);
                        return;
                    }

                    await _qBitService.AddTorrentAsync(p[0], p[1]);
                }
            }
            await _telegramBotService.ReplyAsync(update, "Search is empty. Try Again.");
        }

        private async Task SendSearchData(Update update, SearchResults results, int searchId, int skipFirst = 0)
        {
            var formatedmessages = FormatSearchResultsMessage(results, searchId, skipFirst);

            foreach (var fm in formatedmessages)
            {
                await _telegramBotService.SendTextMessageWithButtonsAsync(update, fm.message, fm.button);
                await Task.Delay(500);
            }

        }

        private async Task<SearchResults> PollForResults(int searchId)
        {
            var stopwatch = Stopwatch.StartNew();
            SearchResults? results = default;
            while (stopwatch.Elapsed < _serachTimeout)
            {
                results = await _qBitService.GetSearchResultsAsync(searchId);

                if (results != null && results.Status == QBittorrent.Client.SearchJobStatus.Stopped)
                {
                    return results; 
                }
                await Task.Delay(TimeSpan.FromSeconds(2));
            }
            return results;
        }

        private IEnumerable<(string message, InlineKeyboardMarkup button)> FormatSearchResultsMessage(SearchResults results, int searchId, int skipFirst = 0)
        {
            int lastMessageCounter = 0;
            foreach (var item in results.Results.OrderByDescending(o => o.Seeds).Skip(skipFirst).Take(MAX_NUMBER_OF_ENTRIES_TO_RETURN))
            {
                lastMessageCounter++;
                StringBuilder message = new StringBuilder();
                message.AppendLine(item.FileName);
                message.AppendLine("Size: " + FormatFileSize(item.FileSize));
                //message.AppendLine("File URL: " + item.FileUrl);
                message.AppendLine("Seeds/Leechers: " + FormatCount(item.Seeds) + "/" + FormatCount(item.Leechers));
                message.AppendLine("Description: " + item.DescriptionUrl);
                message.AppendLine();

                var urlKey = _qBUrlResolverService.SaveUrl(item.FileUrl);

                var inlineKeyboardMarkup = new InlineKeyboardMarkup
                (
                    new[]
                        {
                            new [] {  InlineKeyboardButton.WithCallbackData("Download", "/qdc=" + urlKey) },
                    }
                );

                if (lastMessageCounter == MAX_NUMBER_OF_ENTRIES_TO_RETURN)
                {
                    inlineKeyboardMarkup = new InlineKeyboardMarkup
                    (
                        new[]
                            {
                                new [] { InlineKeyboardButton.WithCallbackData("Download", "/qdc=" + urlKey) },
                                new [] { InlineKeyboardButton.WithCallbackData("Show more...", "/qbsearch=" + $"{searchId}" + $"$p{MAX_NUMBER_OF_ENTRIES_TO_RETURN + skipFirst}") },
                        }
                    );
                }

                yield return (message.ToString(), inlineKeyboardMarkup);

                //message.AppendLine("Site URL: " + item.SiteUrl);
                //message.AppendLine("Description URL: " + item.DescriptionUrl);
            }
        }

        //private IEnumerable<string> SplitMessage(string message, int maxLength)
        //{
        //    for (int i = 0; i < message.Length; i += maxLength)
        //    {
        //        yield return message.Substring(i, Math.Min(maxLength, message.Length - i));
        //    }
        //}

        private string FormatFileSize(long? fileSize)
        {
            if (fileSize.HasValue)
            {
                var sizeInMB = fileSize.Value / (1024 * 1024); // Convert bytes to megabytes
                return sizeInMB + " MB";
            }
            else
            {
                return "N/A";
            }
        }

        private string FormatCount(long? count)
        {
            if (count.HasValue)
            {
                return count.Value.ToString();
            }
            else
            {
                return "N/A";
            }
        }
    }

   

}
