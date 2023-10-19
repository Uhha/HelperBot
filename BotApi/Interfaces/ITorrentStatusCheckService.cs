namespace BotApi.Interfaces
{
    public interface ITorrentStatusCheckService
    {
        Task ExecuteAsync(CancellationToken stoppingToken);
    }
}
