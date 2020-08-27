using System;
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
        CoinCapMarket,
        ErrorMessageLog
    }

    public enum Command
    {
        ComicSubscribe,
        FincanceSubscribe,
        Coins,
        WakeOnLan,
        Balance,
        BalanceAdd,
        BalanceRemove,
        BalanceDetails,
        Unknown,
    }

    public enum CallbackCommand
    {
        ComicSubscribe,
        Unknown
    }


}
