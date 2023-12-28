using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotDB.Models
{
	public partial class Client
	{
		public int Id { get; set; }
		public int ChatId { get; set; }
		public int Subscription { get; set; }

		//public virtual Subscription SubscriptionNavigation { get; set; }
	}
}
