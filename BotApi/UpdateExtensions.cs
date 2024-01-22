using Telegram.Bot.Types;

namespace BotApi
{
	public static class UpdateExtensions
	{
		public static List<string> ParseParameters(this Update update)
		{
			if (update?.Message == null || string.IsNullOrWhiteSpace(update.Message.Text) || !update.Message.Text.StartsWith("/"))
			{
				return null;
			}

			var commandText = update.Message.Text;
			var commandParser = new CommandParser(commandText);

			return commandParser.Parameters;
		}
	}

	public class CommandParser
	{
		public List<string> Parameters { get; private set; }

		public CommandParser(string text)
		{
			Parameters = new List<string>();
			Parse(text);
		}

		private void Parse(string text)
		{
			if (string.IsNullOrWhiteSpace(text))
			{
				return;
			}

			var parts = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);

			for (int i = 1; i < parts.Length; i++)
			{
				Parameters.Add(parts[i]);
			}
		}
	}
}
