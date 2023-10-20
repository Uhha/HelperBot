using BotApi.Interfaces;
using QBittorrent.Client;

namespace BotApi.Services
{
    public class TorrentStatusCheckService : IHostedService, IDisposable
    {
        private readonly IQBitService _qBitService;
        private readonly ITelegramBotService _telegramBotService;
        private readonly ILogger<TorrentStatusCheckService> _logger;
        private Timer _timer;

        public TorrentStatusCheckService(IQBitService qBitService, ITelegramBotService telegramBotService, ILogger<TorrentStatusCheckService> logger)
        {
            _qBitService = qBitService;
            _telegramBotService = telegramBotService;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"TorrentStatusCheckService StartAsync ran.");
            // Set up a timer to call CheckTorrentStatus every 5 minutes
            _timer = new Timer(async state => await CheckTorrentStatusAsync(), null, TimeSpan.Zero, TimeSpan.FromMinutes(5));
            
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        private async Task CheckTorrentStatusAsync()
        {
            try
            {
                _logger.LogInformation($"Checking status in TorrentStatusCheckService");
                var monitoredTorrentHashes = await _qBitService.GetTorrentListAsync();
                foreach (var torrentInfo in monitoredTorrentHashes)
                {
                    _logger.LogInformation($"torrentInfo state = {torrentInfo.State}, hash: {torrentInfo.Hash}");

                    if (torrentInfo != null &&
                        (torrentInfo.State == TorrentState.Uploading ||
                         torrentInfo.State == TorrentState.QueuedUpload ||
                         torrentInfo.State == TorrentState.PausedUpload ||
                         torrentInfo.State == TorrentState.ForcedUpload ||
                         torrentInfo.State == TorrentState.StalledUpload
                         
                         ))
                    {
                        _logger.LogInformation($"Trying to stop upload and notify!");

                        NotifyTorrentFinished(torrentInfo);
                        await _qBitService.DeleteTorrent(torrentInfo.Hash);
                        _qBitService.ActiveTorrents.Remove(torrentInfo.Hash);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in TorrentStatusCheckService: {ex.Message}");
            }
        }

        private void NotifyTorrentFinished(TorrentInfo tinfo)
        {
            if (!_qBitService.ActiveTorrents.ContainsKey(tinfo.Hash))
            {
                _logger.LogInformation($"Torrent {tinfo.Name}, hash: {tinfo.Hash}, is not in the collection.");
            }

            if (_qBitService.ActiveTorrents.ContainsKey(tinfo.Hash))
            {
                var user = _qBitService.ActiveTorrents[tinfo.Hash];
                _logger.LogInformation($"Torrent {tinfo.Name}, user {user} found in Active Torrents list.");

                _telegramBotService.SendTextMessageAsync(user, $"Torrent {tinfo.Name} has finished downloading.");
                _logger.LogInformation($"Torrent {tinfo.Name} has finished downloading.");
            }
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }

    }

}
