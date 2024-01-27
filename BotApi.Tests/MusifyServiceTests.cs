using BotApi.Services;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting.Logging;
using Moq;

namespace BotApi.Tests
{
	[TestClass]
	public class MusifyServiceTests
	{
		[TestMethod]
		public async Task DownloadAlbumTest()
		{
			var mock = new Mock<ILogger<MusifyService>>();
			ILogger<MusifyService> logger = mock.Object;

			var ms = new MusifyService(logger);
			await ms.DownloadAlbumAsync(new Uri("https://musify.club/release/downfall-of-gaia-silhouettes-of-disgust-2023-1718722"));
		}

		[TestMethod]
		public async Task DownloadSongTest()
		{
			var mock = new Mock<ILogger<MusifyService>>();
			ILogger<MusifyService> logger = mock.Object;

			var ms = new MusifyService(logger);
			await ms.DownloadSongAsync(new Uri("https://musify.club/track/dl/19232426/downfall-of-gaia-existence-of-awe.mp3"));
		}
	}
}