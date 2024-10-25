using BotApi.Interfaces;
using System.Globalization;
using System.Text;
using Telegram.Bot.Types;

namespace BotApi.Commands
{
	public class CheckDiskSpaceCommand : BaseCommandAsync
	{
		public CheckDiskSpaceCommand(ITelegramBotService telegramBotService) : base(telegramBotService)
		{
		}

		public override async Task ExecuteAsync(Update update)
		{
			await _telegramBotService.ReplyAsync(update, GetDiskSpaceInfoForTelegram());
		}

        private string GetDiskSpaceInfoForTelegram()
        {
            var drives = DriveInfo.GetDrives();
            var sb = new StringBuilder();

            foreach (var drive in drives)
            {
                if (drive.IsReady && drive.DriveType == DriveType.Fixed)
                {
                    sb.AppendLine($"📁 *Drive:* {drive.Name}");
                    sb.AppendLine($"  - *Total Size:* {FormatBytes(drive.TotalSize)}");
                    sb.AppendLine($"  - *Free Space:* {FormatBytes(drive.AvailableFreeSpace)}");
                    sb.AppendLine();
                }
            }

            return sb.ToString();
        }

        private string FormatBytes(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            double len = bytes;
            int order = 0;

            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len /= 1024;
            }

            return $"{len:0.##} {sizes[order]}";
        }
    }
}
