using BotApi.Interfaces;
using BotApi.Services;
using Microsoft.Extensions.Options;
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

namespace BotApi.Commands
{
    public class QBProgressCommand : BaseCommandAsync
    {
        private readonly IQBitService _qBitService;

        public QBProgressCommand(ITelegramBotService telegramBotService, IQBitService qBitService) :base(telegramBotService) 
        {
            _qBitService = qBitService;
        }

        public override async Task ExecuteAsync(Update update)
        {
            var torrents = await _qBitService.GetTorrentListAsync();
            if (torrents.Count == 0)
            {
                await _telegramBotService.ReplyAsync(update, "No Avtive Torrents Found.");
                return;
            }

            StringBuilder message = new StringBuilder();
            foreach (var torrent in torrents)
            {
                message.AppendLine($"{torrent.Name}, State: {torrent.State}, Progress: {(torrent.Progress * 100).ToString("0") + "%"}");
            }
            message.AppendLine();

            await _telegramBotService.ReplyAsync(update, message.ToString());
        }
    }
}
