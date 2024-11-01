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
			await _telegramBotService.ReplyAsync(update, GetTemperatureWarning());
		}

        public string GetTemperatureWarning(double thresholdCelsius = 75.0)
        {
            var sb = new StringBuilder();
            string command = "vcgencmd measure_temp";

            // Execute the command and capture the output
            string output = ExecuteCommand(command);

            if (output != null)
            {
                // Extract the temperature value from the output
                string tempString = output.Replace("temp=", "").Replace("'C", "").Trim();
                sb.Append(tempString);

                //if (double.TryParse(tempString, out double temperatureCelsius))
                //{
                //    // Check if the temperature exceeds the threshold
                //    if (temperatureCelsius > thresholdCelsius)
                //    {
                //        sb.AppendLine($"⚠️ *Warning!* High CPU temperature detected!");
                //        sb.AppendLine($"  - *Temperature:* {temperatureCelsius:0.##} °C");
                //    }
                //    else
                //    {
                //        sb.AppendLine($"✅ *Temperature is normal:* {temperatureCelsius:0.##} °C");
                //    }
                //}
                //else
                //{
                //    sb.AppendLine("❌ *Error parsing temperature value.*");
                //}
            }
            else
            {
                sb.AppendLine("❌ *Error executing temperature command.*");
            }

            return sb.ToString();
        }

        private string ExecuteCommand(string command)
        {
            try
            {
                // Create a new process to run the command
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "/bin/bash",
                        Arguments = $"-c \"{command}\"",
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };

                process.Start();
                string result = process.StandardOutput.ReadToEnd();
                process.WaitForExit();
                return result;
            }
            catch (Exception ex)
            {
                return null; // or log the exception
            }
        }
    }
}
