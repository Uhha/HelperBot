using BotDB.Models;
using Microsoft.EntityFrameworkCore;

namespace BotDB
{
	public class BotDbContext: DbContext
	{
        public BotDbContext(DbContextOptions<BotDbContext> options): base(options)
        {
        }

		public virtual DbSet<Client> Clients { get; set; }
	}
}