using System;
using Telegram.Bot;
using Telegram.Bot.Types;
using Mono.Data.Sqlite;
using System.IO;
using System.Collections.Generic;
using DatabaseInteractions;
using System.Data.Entity;

namespace Logic.Processors
{
    internal class DocumentMessageCommandProcessor
    {
        
        internal static void Process(TelegramBotClient bot, Update update)
        {
            if (update.Message.Document.FileName.Equals("vocab.db"))
            {
                try
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
                }
                catch (Exception e)
                {
                    var asd = e.Message;
                    throw;
                }

                //get words from SQLite file
                LinkedList<Words> words = new LinkedList<Words>();
                LinkedList<Lookups> lookups = new LinkedList<Lookups>();
                LinkedList<BookInfo> bookinfo = new LinkedList<BookInfo>();
                try
                {
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
                                //bot.SendTextMessageAsync(update.Message.Chat.Id, $"Extracted {words.Count.ToString()} words from the vocab.db");
                            }
                        }

                        stm = "SELECT * FROM Lookups";

                        using (SqliteCommand cmd = new SqliteCommand(stm, con))
                        {
                            using (SqliteDataReader rdr = cmd.ExecuteReader())
                            {
                                while (rdr.Read())
                                {
                                    lookups.AddFirst
                                    (
                                        new Lookups
                                        {
                                            CRid = rdr.GetString(0),
                                            WordID = rdr.GetString(1),
                                            BookKey = rdr.GetString(2),
                                            DicKey = rdr.GetString(3),
                                            Pos = rdr.GetString(4),
                                            Usage = rdr.GetString(5),
                                            Timestamp = rdr.GetInt32(6)
                                        }
                                    );
                                }
                            }
                        }

                        stm = "SELECT * FROM BOOK_INFO";

                        using (SqliteCommand cmd = new SqliteCommand(stm, con))
                        {
                            using (SqliteDataReader rdr = cmd.ExecuteReader())
                            {
                                while (rdr.Read())
                                {
                                    bookinfo.AddFirst
                                    (
                                        new BookInfo
                                        {
                                            CRid = rdr.GetString(0),
                                            asin = rdr.GetString(1),
                                            guid = rdr.GetString(2),
                                            lang = rdr.GetString(3),
                                            title = rdr.GetString(4),
                                            authors = rdr.GetString(5)
                                        }
                                    );
                                }
                            }
                        }
                        con.Close();
                    }
                }
                catch (Exception e2)
                {
                    var asdad = e2.Message;
                    throw;
                }
                bot.SendTextMessageAsync(update.Message.Chat.Id, $"Extracted {words.Count.ToString()} words from the vocab.db");

                //Add words to SQL Server
                using (AlcoDBEntities db = new AlcoDBEntities())
                {
                    db.Database.ExecuteSqlCommand("delete from Words");
                    db.Words.AddRange(words);
                    db.Database.ExecuteSqlCommand("delete from Lookups");
                    db.Lookups.AddRange(lookups);
                    db.Database.ExecuteSqlCommand("delete from BookInfo");
                    db.BookInfo.AddRange(bookinfo);

                    db.SaveChanges();
                    bot.SendTextMessageAsync(update.Message.Chat.Id, $"Added {words.Count.ToString()} words to the DB!");
                }

            }

        }
       
    }


}