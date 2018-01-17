﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logic
{
    public enum Subscription
    {
        NoSubscription,
        Oglaf,
        XKCD,
        CoinCapMarket
        
    }

    public enum VocabCallbackType
    {
        Word,
        Definition
    }

    public enum Command
    {
        ComicSubscribe,
        FincanceSubscribe,
        Coins,
        Vocabulary,
        DefineWord,
        WakeOnLan,
        Trend,
        Balance,
        BalanceAdd,
        BalanceRemove,
        Unknown,
        BalanceDetails
    }

    public enum CallbackCommand
    {
        ComicSubscribe,
        VocabNewWord,
        VocabDefinition,
        Unknown
    }


}
