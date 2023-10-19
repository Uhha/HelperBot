using BotApi.Interfaces;
using QBittorrent.Client;

namespace BotApi.Services
{
    public class TorrentStatusCheckService : BackgroundService, ITorrentStatusCheckService
    {
        private readonly IQBitService _qBitService;
        private readonly ILogger<TorrentStatusCheckService> _logger;
        private readonly ITelegramBotService _telegramBotService;

        public TorrentStatusCheckService(IQBitService qBitService, ITelegramBotService telegramBotService, ILogger<TorrentStatusCheckService> logger)
        {
            _qBitService = qBitService;
            _telegramBotService = telegramBotService;
            _logger = logger;
        }

        async Task ITorrentStatusCheckService.ExecuteAsync(CancellationToken stoppingToken)
        {
            await ExecuteAsync(stoppingToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var monitoredTorrentHashes = await _qBitService.GetTorrentListAsync();
                    foreach (var torrentInfo in monitoredTorrentHashes)
                    {
                        if (torrentInfo != null && 
                            (torrentInfo.State == QBittorrent.Client.TorrentState.Uploading ||
                            torrentInfo.State == QBittorrent.Client.TorrentState.QueuedUpload ||
                            torrentInfo.State == QBittorrent.Client.TorrentState.PausedUpload ||
                            torrentInfo.State == QBittorrent.Client.TorrentState.ForcedUpload
                            )
                        )
                        {
                            NotifyTorrentFinished(torrentInfo);
                            await _qBitService.DeleteTorrent(torrentInfo.Hash);
                        }
                    }
                    await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    return;
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in TorrentStatusCheckService: {ex.Message}");
                }
            }
        }

        private void NotifyTorrentFinished(TorrentInfo tinfo)
        {
            if (_qBitService.ActiveTorrents.ContainsKey(tinfo.Hash))
            {
                var user = _qBitService.ActiveTorrents[tinfo.Hash];
                _telegramBotService.SendTextMessageAsync(user, $"Torrent {tinfo.Name} has finished downloading.");
                _logger.LogInformation($"Torrent {tinfo.Name} has finished downloading.");
            }
        }

        
    }

}
