using Microsoft.EntityFrameworkCore;

namespace app.Infrastructure
{
	public class AppDbContext : DbContext
	{
		public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
		{
		}

		// 範例 DbSet，請依需求新增
		// public DbSet<Player> Players { get; set; }
		// public DbSet<Game> Games { get; set; }
	}
}
