using BotApi.Interfaces;
using Microsoft.Extensions.ObjectPool;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using Telegram.Bot.Types;

namespace BotApi.Commands
{
	public class CheckDiscTempCommand : BaseCommandAsync
	{
		public CheckDiscTempCommand(ITelegramBotService telegramBotService) : base(telegramBotService)
		{
		}

		public override async Task ExecuteAsync(Update update)
		{
			await _telegramBotService.ReplyAsync(update, GetTemperatureWarningFallback());
		}

        public string GetTemperatureWarningFallback(double thresholdCelsius = 75.0)
        {
            string temperatureFilePath = "/sys/class/thermal/thermal_zone0/temp";
            var sb = new StringBuilder();

            if (System.IO.File.Exists(temperatureFilePath))
            {
                string tempString = System.IO.File.ReadAllText(temperatureFilePath).Trim();
                if (double.TryParse(tempString, out double temperatureMillidegrees))
                {
                    double temperatureCelsius = temperatureMillidegrees / 1000.0;
                    if (temperatureCelsius > thresholdCelsius)
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
