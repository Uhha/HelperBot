using System;
using Telegram.Bot;
using Telegram.Bot.Types;
using Mono.Data.Sqlite;
using System.IO;
using System.Collections.Generic;
using DatabaseInteractions;
using System.Data.Entity;

namespace Logic
{
    internal class DocumentMessageCommandProcessor
    {
        
        internal static void Process(TelegramBotClient bot, Update update)
        {
            if (update.Message.Document.FileName.Equals("vocab.db"))
            {
                //Save a db file locally
                var file = bot.GetFileAsync(update.Message.Document.FileId);
                using (Stream ff = System.IO.File.Create("temp.db"))
                {
                    CopyStream(file.Result.FileStream, ff);
                    void CopyStream(Stream input, Stream output)
                    {
                        byte[] buffer = new byte[8 * 1024];
                        int len;
                        while ((len = input.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            output.Write(buffer, 0, len);
                        }
                    }
                }

                //get words from SQLite file
                LinkedList<Words> words = new LinkedList<Words>();
                using (SqliteConnection con = new SqliteConnection("URI = file:temp.db"))
                {
                    con.Open();
                    string stm = "SELECT * FROM Words";

                    using (SqliteCommand cmd = new SqliteCommand(stm, con))
                    {
                        using (SqliteDataReader rdr = cmd.ExecuteReader())
                        {
                            while (rdr.Read())
                            {
                                words.AddFirst
                                (
                                    new Words
                                    {
                                        WordID = rdr.GetString(0),
                                        Word = rdr.GetString(1),
                                        Stem = rdr.GetString(2),
                                        Lang = rdr.GetString(3),
                                        Category = rdr.GetInt32(4),
                                        Timestamp = rdr.GetInt32(5),
                                        ProfileID = rdr.GetString(6)
                                    }                                    
                                );
                            }
                            bot.SendTextMessageAsync(update.Message.Chat.Id, $"Extracted {words.Count.ToString()} words from the vocab.db");
                        }
                    }

                    con.Close();
                }

                //Add words to SQL Server
                using (AlcoDBEntities db = new AlcoDBEntities())
                {
                    db.Database.ExecuteSqlCommand("delete from Words");
                    db.Words.AddRange(words);
                    db.SaveChanges();
                    bot.SendTextMessageAsync(update.Message.Chat.Id, $"Added {words.Count.ToString()} words to the DB!");
                }

            }

        }
       
    }


}