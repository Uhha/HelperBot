using BotApi.Interfaces;
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
			var date = update.Message.Text.Substring(update.Message.Text.IndexOf(' ') + 1);
			
			if (date is null)
			{
				var today = DateTime.Now.ToString("yyyyMMdd");
				var filepath = Path.Combine(LOGS_PATH_PREFIX, today, ".log");

				await _telegramBotService.SendFileAsync(update, filepath);
				return;
			}
			
			if (DateTime.TryParse(date, out DateTime requestedDate))
			{
				var filepath = Path.Combine(LOGS_PATH_PREFIX, requestedDate.ToString("yyyyMMdd"), ".log");

				await _telegramBotService.SendFileAsync(update, filepath);
				return;
			}
		}
	}
}
