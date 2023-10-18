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
    public class QBPluginsCommand : BaseCommandAsync
    {
        private readonly IQBitService _qBitService;

        public QBPluginsCommand(ITelegramBotService telegramBotService, IQBitService qBitService) :base(telegramBotService) 
        {
            _qBitService = qBitService;
        }

        public override async Task ExecuteAsync(Update update)
        {
            var plugins = await _qBitService.GetSearchPluginsAsync();
            
            if (plugins.Count > 0)
            {
                var message = FormatSearchResultsMessage(plugins);
                await _telegramBotService.ReplyAsync(update, message);
                return;
            }
            await _telegramBotService.ReplyAsync(update, "No Search Plugins Found.");
        }

        private string FormatSearchResultsMessage(IEnumerable<SearchPlugin> results)
        {
            StringBuilder message = new StringBuilder();
            foreach (var plugin in results)
            {
                message.AppendLine($"{ plugin.Name } | { (plugin.IsEnabled ? "enabled" : "disabled") }");
            }

            return message.ToString();
        }

      
    }

    public class QBEnablePluginCommand : BaseCommandAsync
    {
        private readonly IQBitService _qBitService;

        public QBEnablePluginCommand(ITelegramBotService telegramBotService, IQBitService qBitService) : base(telegramBotService)
        {
            _qBitService = qBitService;
        }

        public override async Task ExecuteAsync(Update update)
        {
            var indexOfParam = update.Message.Text.IndexOf(' ');
            if (indexOfParam <= 0)
            {
                await _telegramBotService.ReplyAsync(update, $"Empty parameter - plugin name.");
                return;
            }

            var pluginName = update.Message.Text.Substring(indexOfParam + 1);

            if (pluginName.Length > 0)
            {
                try
                {
                    await _qBitService.EnablePluginAsync(pluginName);
                }
                catch (Exception e)
                {
                    await _telegramBotService.ReplyAsync(update, $"{e.Message}");
                }
                await _telegramBotService.ReplyAsync(update, $"{pluginName} Enabled.");
            }
        }
    }

    public class QBDisablePluginCommand : BaseCommandAsync
    {
        private readonly IQBitService _qBitService;

        public QBDisablePluginCommand(ITelegramBotService telegramBotService, IQBitService qBitService) : base(telegramBotService)
        {
            _qBitService = qBitService;
        }

        public override async Task ExecuteAsync(Update update)
        {
            var indexOfParam = update.Message.Text.IndexOf(' ');
            if (indexOfParam <= 0) 
            {
                await _telegramBotService.ReplyAsync(update, $"Empty parameter - plugin name.");
                return;
            }

            var pluginName = update.Message.Text.Substring(indexOfParam + 1);

            if (pluginName.Length > 0)
            {
                try
                {
                    await _qBitService.DisablePluginAsync(pluginName);
                }
                catch (Exception e)
                {
                    await _telegramBotService.ReplyAsync(update, $"{e.Message}");
                }
                await _telegramBotService.ReplyAsync(update, $"{pluginName} Disabled.");
            }
        }
    }


}
