namespace BotApi.Interfaces
{
	public interface IMusifyService
	{
		public void DownloadAlbum(Uri url);
		public void DownloadSong(Uri url);
	}
}
