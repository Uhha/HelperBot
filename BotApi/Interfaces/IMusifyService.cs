namespace BotApi.Interfaces
{
	public interface IMusifyService
	{
		public Task<string> DownloadAlbumAsync(Uri url);
		public Task<string> DownloadSongAsync(Uri url);
	}
}
