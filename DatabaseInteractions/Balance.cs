﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
using System;
using System.Collections.Generic;

namespace DatabaseInteractions
{
    public partial class Balance
    {
        public int Id { get; set; }
        public int Client { get; set; }
        public string Symbol { get; set; }
        public decimal? Shares { get; set; }
    }
}