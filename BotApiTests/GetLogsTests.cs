using BotApi.Commands;
using BotApi.Interfaces;
using BotApi.Services;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting.Logging;
using Moq;
using Telegram.Bot.Types;

namespace BotApiTests
{
	[TestClass]
	public class GetLogsTests
	{
		//[TestMethod]
		//public async Task DownloadAlbumTest()
		//{
		//	var mock = new Mock<TelegramBotService>();
		//	TelegramBotService telegram = mock.Object;

		//	var gl = new GetLogsCommand(telegram);
		//	await gl.ExecuteAsync()
		//}

		[TestMethod]
		public async Task ExecuteAsync_NoDateProvided_SendFileWithTodayDate()
		{
			// Arrange
			var update = new Update
			{
				Message = new Message
				{
					Text = "/logs" // Replace with the actual command that triggers this method
				}
			};

			var mockTelegramBotService = new Mock<ITelegramBotService>();
			var gl = new GetLogsCommand(mockTelegramBotService.Object);

			// Act
			await gl.ExecuteAsync(update);

			// Assert
			mockTelegramBotService.Verify(
				service => service.SendFileAsync(
					It.IsAny<Update>(),
					It.Is<string>(filepath => filepath.Contains(DateTime.Now.ToString("yyyyMMdd")))
				),
				Times.Once
			);
		}

		[TestMethod]
		public async Task ExecuteAsync_ValidDateProvided_SendFileWithRequestedDate()
		{
			// Arrange
			var update = new Update
			{
				Message = new Message
				{
					Text = "/logs 01212024" // Replace with the actual command and date
				}
			};

			var mockTelegramBotService = new Mock<ITelegramBotService>();
			var gl = new GetLogsCommand(mockTelegramBotService.Object);

			// Act
			await gl.ExecuteAsync(update);

			// Assert
			mockTelegramBotService.Verify(
				service => service.SendFileAsync(
					It.IsAny<Update>(),
					It.Is<string>(filepath => filepath.Contains("20240121"))
				),
				Times.Once
			);
		}

		[TestMethod]
		public async Task ExecuteAsync_InvalidDateProvided_DoNotSendFile()
		{
			// Arrange
			var update = new Update
			{
				Message = new Message
				{
					Text = "/logs invalidDate" // Replace with the actual command and invalid date
				}
			};

			var mockTelegramBotService = new Mock<ITelegramBotService>();
			var gl = new GetLogsCommand(mockTelegramBotService.Object);

			// Act
			await gl.ExecuteAsync(update);

			// Assert
			mockTelegramBotService.Verify(
				service => service.SendFileAsync(It.IsAny<Update>(), It.IsAny<string>()),
				Times.Never
			);
		}
	}
}