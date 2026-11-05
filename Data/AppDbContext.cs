using Microsoft.EntityFrameworkCore;
using Real_Time_Stock_Price_Monitor.Models;

namespace Real_Time_Stock_Price_Monitor.Data
{
    public class AppDbContext :DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<AppUser> Users => Set<AppUser>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AppUser>().HasIndex(u => u.Username).IsUnique();
        }
    }
}
