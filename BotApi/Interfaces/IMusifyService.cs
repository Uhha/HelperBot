namespace BotApi.Interfaces
{
	public interface IMusifyService
	{
		public Task DownloadAlbumAsync(Uri url);
		public Task DownloadSongAsync(Uri url);
	}
}
