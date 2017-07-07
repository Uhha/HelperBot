using DatabaseInteractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace Logic
{
    public static class VocabCallbackData
    {
        private static string[] _words;
        private static int index;
        public static string Word
        {
            get
            {
                if (_words == null) SetWords();
                if (index < _words.Length) { return _words[index++]; } else { return _words[index = 0]; }
            }
            set { }
        }

        private static void SetWords()
        {
            using (AlcoDBEntities db = new AlcoDBEntities())
            {
                IQueryable<string> wrds = from p in db.Words
                                          select p.Stem;
                Random rnd = new Random();
                _words = wrds.ToArray().OrderBy(x => rnd.Next()).ToArray();
            }
        }



    }
}
