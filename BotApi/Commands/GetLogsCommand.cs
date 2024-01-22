using BotApi.Interfaces;
using System.Globalization;
using Telegram.Bot.Types;

namespace BotApi.Commands
{
	public class GetLogsCommand : BaseCommandAsync
	{
		private string LOGS_PATH_PREFIX = "logs/app";
		public GetLogsCommand(ITelegramBotService telegramBotService) : base(telegramBotService)
		{
		}

		public override async Task ExecuteAsync(Update update)
		{
			var parameter = update.ParseParameters();
			
			if (parameter.Count() == 0)
			{
				var today = DateTime.Now.ToString("yyyyMMdd");
				var filepath = LOGS_PATH_PREFIX + today + ".txt";

				await _telegramBotService.SendFileAsync(update, filepath);
				return;
			}

			if (DateTime.TryParseExact(parameter.First(), "MMddyyyy", null, DateTimeStyles.None, out DateTime requestedDate))
			{
				var filepath = LOGS_PATH_PREFIX + requestedDate.ToString("yyyyMMdd") + ".log";

				await _telegramBotService.SendFileAsync(update, filepath);
				return;
			}

			await _telegramBotService.ReplyAsync(update, "Can't parse the command");
		}
	}
}
