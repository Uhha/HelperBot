using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace Logic
{
    public class VocabCallbackData : CallbackGame
    {
        public GameType gameType = GameType.VocabGame;
        public VocabCallbackType vocabCallbackType;
        public string[] words;
        private int index;
        public string Word
        {
            get
            {
                if (index < words.Length) { return words[index++]; } else { return words[index = 0]; }
            }
            set { }
        }

        public override string ToString()
        {
            return Word.ToString();
        }

    }
}
