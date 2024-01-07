using BotApi.Interfaces;
using HtmlAgilityPack;

namespace BotApi.Services
{
	public class MusifyService : IMusifyService
	{
		private const string MUSIC_FOLDER = "music";
		private const string SINGLE_FILE_FOLDER = "Random";
		private readonly ILogger<MusifyService> _logger;
        private readonly HttpClient httpClient;

		public MusifyService(ILogger<MusifyService> logger)
        {
			_logger = logger;
			httpClient = new HttpClient();
            httpClient.Timeout = TimeSpan.FromSeconds(300);
        }
        public async Task<string> DownloadAlbumAsync(Uri url)
		{
			return await DownloadMp3Files(url, MUSIC_FOLDER);
		}

		public async Task<string> DownloadSongAsync(Uri url)
		{
			return await DownloadSingleMp3File(url, MUSIC_FOLDER);
		}

		private async Task<string> DownloadMp3Files(Uri url, string volumePath)
		{
			try
			{
				var html = await httpClient.GetStringAsync(url);

				var htmlDocument = new HtmlDocument();
				htmlDocument.LoadHtml(html);

				var header = htmlDocument.DocumentNode.SelectSingleNode("//header[@class='content__title']/h1");
				var title = header?.InnerText.Trim().Split(" - ");

				var bandName = title[0].Trim();
				var albumName = title[1].Trim();

				var mp3Links = htmlDocument.DocumentNode.SelectNodes("//div[contains(@class, 'playlist__item')]//a[@download]");

				if (mp3Links != null && mp3Links.Any() && !string.IsNullOrEmpty(bandName) && !string.IsNullOrEmpty(albumName))
				{
					string bandFolder = CleanFolderName(bandName);
					string albumFolder = CleanFolderName(albumName);
					string savePath = Path.Combine(volumePath, bandFolder, albumFolder);

					Directory.CreateDirectory(savePath);

					foreach (var link in mp3Links)
					{
						string mp3Url = link.GetAttributeValue("href", string.Empty);
						string mp3FileName = link.GetAttributeValue("download", string.Empty);

						if (!string.IsNullOrEmpty(mp3Url) && !string.IsNullOrEmpty(mp3FileName))
						{
							string filePath = Path.Combine(savePath, mp3FileName);
							var success = await DownloadFile(new Uri(url, mp3Url), filePath);

                            if (!success)
                            {
                                _logger.LogError($"Could not download {mp3FileName}");
                            }
                        }
					}
					return $"{bandFolder} - {albumFolder}";
				}
				else
				{
					_logger.LogWarning("No MP3 links found on the provided URL, or band/album names could not be extracted.");
					return "";
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, ex.Message);
			}
            return "";
        }

        private async Task<string> DownloadSingleMp3File(Uri mp3Link, string volumePath)
		{
			try
			{
				string savePath = Path.Combine(volumePath, SINGLE_FILE_FOLDER);

				Directory.CreateDirectory(savePath);

				string mp3FileName = Path.GetFileName(mp3Link.LocalPath);
				string filePath = Path.Combine(savePath, mp3FileName);

				var success = await DownloadFile(mp3Link, filePath);


                if (!success)
				{
                    _logger.LogError($"Could not download {mp3FileName}");
                }

				return mp3FileName;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, ex.Message);
			}
			return "";
		}

		private string CleanFolderName(string folderName)
		{
			char[] invalidChars = Path.GetInvalidFileNameChars();
			return string.Join("_", folderName.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries)).TrimEnd('.');
		}

		private async Task<bool> DownloadFile(Uri fileUri, string fileName)
		{
            int currentAttempt = 1;
			int maxAttempts = 3;

            while (currentAttempt < maxAttempts)
			{

				try
				{
					var response = await httpClient.GetAsync(fileUri);

					if (response.IsSuccessStatusCode)
					{
						using var fileStream = await response.Content.ReadAsStreamAsync();
						using var file = new System.IO.FileStream(fileName, System.IO.FileMode.Create);

						await fileStream.CopyToAsync(file);
						_logger.LogInformation($"File '{fileName}' downloaded successfully.");
                        return true;
                    }
					else
					{
						_logger.LogWarning($"Failed to download file '{fileName}'. Status code: {response.StatusCode}");
					}
				}
                catch (TaskCanceledException)
                {
                    _logger.LogWarning($"Download attempt {currentAttempt} timed out. Retrying...");
                }
                catch (Exception ex)
				{
					_logger.LogError($"An error occurred while downloading file '{fileName}': {ex.Message}");
					break;
                }
				currentAttempt++;
			}
			return false;
		}
	}
}
