using BotApi.Interfaces;
using QBittorrent.Client;

namespace BotApi.Services
{
    public class QBitService: IQBitService
    {
        private readonly QBittorrentClient _qBittorrentClient;

        public QBitService(string url)
        {
            _qBittorrentClient = new QBittorrentClient(new Uri(url));
        }
    }
}
