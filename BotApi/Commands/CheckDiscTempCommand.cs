using BotApi.Interfaces;
using System.Text;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace BotApi.Commands
{
	public class CheckDiscTempCommand : BaseCommandAsync
	{
        private static readonly string _temperatureFilePath = "/sys/class/thermal/thermal_zone0/temp";
        private static readonly double _thresholdCelsius = 75.0;
        private static int? _lastMessageId;
        private static DateTime _dateSent = DateTime.MinValue;

        public CheckDiscTempCommand(ITelegramBotService telegramBotService) : base(telegramBotService)
		{
		}

		public override async Task ExecuteAsync(Update update)
		{
            string newMessage = GetTemperatureWarningFallback();

            int minutesPassed = (int)(DateTime.Now - _dateSent).TotalMinutes;

            if (minutesPassed < 5 && _lastMessageId.HasValue)
            {
                await _telegramBotService.EditMessageAsync(update.Message.Chat.Id, _lastMessageId.Value, newMessage, ParseMode.Markdown);
            }
            else
            {
                var message = await _telegramBotService.ReplyAsync(update, newMessage);
                _lastMessageId = message.MessageId;
                _dateSent = message.Date;
            }
		}

        public string GetTemperatureWarningFallback()
        {
            var sb = new StringBuilder();

            if (System.IO.File.Exists(_temperatureFilePath))
            {
                string tempString = System.IO.File.ReadAllText(_temperatureFilePath).Trim();
                if (double.TryParse(tempString, out double temperatureMillidegrees))
                {
                    double temperatureCelsius = temperatureMillidegrees / 1000.0;
                    if (temperatureCelsius > _thresholdCelsius)
                    {
                        sb.AppendLine($"⚠️ *Warning!* High CPU temperature detected!");
                        sb.AppendLine($"  - *Temperature:* {temperatureCelsius:0.##} °C");
                    }
                    else
                    {
                        sb.AppendLine($"✅ *Temperature is normal:* {temperatureCelsius:0.##} °C");
                    }
                }
                else
                {
                    sb.AppendLine("❌ *Error parsing temperature value.*");
                }
            }
            else
            {
                sb.AppendLine("❌ *Temperature file not found.*");
            }

            return sb.ToString();
        }
    }
}
