using FintachartsAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace FintachartsAPI.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Asset> Assets { get; set; }
        public DbSet<AssetPrice> AssetPrices { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<AssetPrice>()
                .Property(p => p.Price)
                .HasColumnType("decimal(18,6)");
        }
    }
}