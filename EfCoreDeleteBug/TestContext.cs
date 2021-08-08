using Microsoft.EntityFrameworkCore;

namespace EfCoreDeleteBug
{
    public class TestContext : DbContext
    {
        public DbSet<Child> Children { get; private set; } = null!;
        public DbSet<Parent> Parents { get; private set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder dbContextOptionsBuilder) => dbContextOptionsBuilder
            .UseSqlite("Data Source=Application.db;");

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Parent>(x =>
            {
                x.HasMany(p => p.Children)
                    .WithOne(c => c.Parent!)
                    .OnDelete(DeleteBehavior.Cascade);
                var f = x.HasOne(p => p.SpecialChild)
                    .WithOne()
                    .HasForeignKey<Parent>(p => p.SpecialChildId)
                    .OnDelete(DeleteBehavior.NoAction);
            });
        }
    }
}
