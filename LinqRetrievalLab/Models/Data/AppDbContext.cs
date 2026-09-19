using LinqRetrievalLab.Models.Domain;
using Microsoft.EntityFrameworkCore;

namespace LinqRetrievalLab.Models.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        public DbSet<Product> Product => Set<Product>();

        public DbSet<Category> Category => Set<Category>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

        }
    }
}