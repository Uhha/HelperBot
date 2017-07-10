using Logic;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Telegram.Bot;

namespace Web
{
    public static class NoHookLoop
    {
        public static void Start()
        {
            Run().Wait();
        }

        static async Task Run()
        {
            var TelegramBotClient = new TelegramBotClient(Config.TestBotApiKey);

            var me = await TelegramBotClient.GetMeAsync();

            //Console.WriteLine("Hello my name is {0}", me.Username);

            var offset = 0;
            
            while (true)
            {
                var updates = await TelegramBotClient.GetUpdatesAsync(offset);

                foreach (var update in updates)
                {
                    //if (update.Message.Text != null)
                    //	await TelegramBotClient.SendTextMessageAsync(update.Message.Chat.Id, update.Message.Text);

                    //if (update.Message.Photo != null)
                    //{
                    //	var file = await TelegramBotClient.GetFileAsync(update.Message.Photo.LastOrDefault()?.FileId);

                    //	var filename = file.FileId + "." + file.FilePath.Split('.').Last();

                    //	using (var profileImageStream = File.Open(filename, FileMode.Create))
                    //	{
                    //		await file.FileStream.CopyToAsync(profileImageStream);
                    //	}


                    //}

                    Task.Run(() => new Handler().Handle(update));

                    offset = update.Id + 1;
                }

                await Task.Delay(1000);
            }
        }

    }
}