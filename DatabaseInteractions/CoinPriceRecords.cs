//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace DatabaseInteractions
{
    using System;
    using System.Collections.Generic;
    
    public partial class CoinPriceRecords
    {
        public int ID { get; set; }
        public Nullable<System.DateTime> dtRecorded { get; set; }
        public string CoinSymbol { get; set; }
        public Nullable<decimal> Price { get; set; }
    }
}