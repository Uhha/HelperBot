﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
using System;
using System.Collections.Generic;

namespace DatabaseInteractions
{
    public partial class Subscription
    {
        public Subscription()
        {
            Clients = new HashSet<Client>();
        }

        public int Id { get; set; }
        public int SubsctiptionType { get; set; }
        public string LastPostedKey { get; set; }

        public virtual ICollection<Client> Clients { get; set; }
    }
}