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
    }
}