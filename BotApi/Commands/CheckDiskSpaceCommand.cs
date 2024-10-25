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
            var seenDrives = new HashSet<string>();
            var sb = new StringBuilder();

            foreach (var drive in drives)
            {
                // Only include real file systems and skip if we've already seen this physical drive
                if (drive.IsReady && drive.DriveType == DriveType.Fixed)
                {
                    // Check if we have already recorded this drive's root path
                    var rootPath = Path.GetPathRoot(drive.RootDirectory.FullName);

                    if (seenDrives.Contains(rootPath))
                    {
                        continue; // Skip duplicate mount
                    }

                    // Add root path to seen list to avoid duplicates
                    seenDrives.Add(rootPath);

                    sb.AppendLine($"📁 *Drive:* {drive.VolumeLabel}");
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
