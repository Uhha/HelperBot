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
    public class MusifyDownloadAlbumCommand : BaseCommandAsync
    {
        private IMusifyService _musifyService;
        private ILogger<MusifyDownloadAlbumCommand> _logger;

        public MusifyDownloadAlbumCommand(ITelegramBotService telegramBotService, 
            IMusifyService musifyService,
            ILogger<MusifyDownloadAlbumCommand> logger) :base(telegramBotService) 
        {
            _musifyService = musifyService;
            _logger = logger;
        }

        public override async Task ExecuteAsync(Update update)
        {
			var url = update.Message?.Text?.Substring(update.Message.Text.IndexOf(' ') + 1);
            if (!string.IsNullOrWhiteSpace(url))
            {
                string title = string.Empty;
                try
                {
					title = await _musifyService.DownloadAlbumAsync(new Uri(url));
				}
				catch (Exception ex)
				{
					_logger.LogError(ex.Message);
					await _telegramBotService.ReplyAsync(update, "Something went wrong: " + ex.Message);
					return;
				}
				await _telegramBotService.ReplyAsync(update, $"{title} Album Download Complete.");
            }
        }
      
    }

	public class MusifyDownloadSongCommand : BaseCommandAsync
	{
		private IMusifyService _musifyService;
		private ILogger<MusifyDownloadSongCommand> _logger;

		public MusifyDownloadSongCommand(ITelegramBotService telegramBotService,
			IMusifyService musifyService,
			ILogger<MusifyDownloadSongCommand> logger) : base(telegramBotService)
		{
			_musifyService = musifyService;
			_logger = logger;
		}

		public override async Task ExecuteAsync(Update update)
		{
			var url = update.Message?.Text?.Substring(update.Message.Text.IndexOf(' ') + 1);
			if (!string.IsNullOrWhiteSpace(url))
			{
				string title = string.Empty;
				try
				{
					title = await _musifyService.DownloadSongAsync(new Uri(url));
				}
				catch (Exception ex)
				{
					_logger.LogError(ex.Message);
					await _telegramBotService.ReplyAsync(update, "Something went wrong: " + ex.Message);
					return;
				}
				await _telegramBotService.ReplyAsync(update, $"{title} Download Complete.");
			}
		}

	}




}
