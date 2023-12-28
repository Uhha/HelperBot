using BotApi.Interfaces;
using HtmlAgilityPack;

namespace BotApi.Services
{
	public class MusifyService : IMusifyService
	{
		private readonly HttpClient httpClient;

		public MusifyService()
        {
			httpClient = new HttpClient();
		}
        public async Task DownloadAlbum(Uri url)
		{
			await DownloadMp3Files(url);
		}

		public async Task DownloadSong(Uri url)
		{
			throw new NotImplementedException();
		}

		private async Task DownloadMp3Files(Uri url)
		{
			try
			{
				var html = await httpClient.GetStringAsync(url);

				var htmlDocument = new HtmlDocument();
				htmlDocument.LoadHtml(html);

				var mp3Links = htmlDocument.DocumentNode.SelectNodes("//div[contains(@class, 'playlist__item')]//a[@download]");

				if (mp3Links != null && mp3Links.Any())
				{
					foreach (var link in mp3Links)
					{
						string mp3Url = link.GetAttributeValue("href", string.Empty);
						string mp3FileName = link.GetAttributeValue("download", string.Empty);

						if (!string.IsNullOrEmpty(mp3Url) && !string.IsNullOrEmpty(mp3FileName))
						{
							await DownloadFile(new Uri(url, mp3Url), mp3FileName);
						}
					}
				}
				else
				{
					Console.WriteLine("No MP3 links found on the provided URL.");
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"An error occurred: {ex.Message}");
			}
		}

		private async Task DownloadFile(Uri fileUri, string fileName)
		{
			try
			{
				var response = await httpClient.GetAsync(fileUri);

				if (response.IsSuccessStatusCode)
				{
					using var fileStream = await response.Content.ReadAsStreamAsync();
					using var file = new System.IO.FileStream(fileName, System.IO.FileMode.Create);

					await fileStream.CopyToAsync(file);
					Console.WriteLine($"File '{fileName}' downloaded successfully.");
				}
				else
				{
					Console.WriteLine($"Failed to download file '{fileName}'. Status code: {response.StatusCode}");
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"An error occurred while downloading file '{fileName}': {ex.Message}");
			}
		}
	}
}
