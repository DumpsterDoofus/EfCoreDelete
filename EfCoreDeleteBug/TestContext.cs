using Microsoft.EntityFrameworkCore;

namespace EfCoreDeleteBug
{
    public class TestContext : DbContext
    {
        public DbSet<Child> Children { get; set; } = null!;
        public DbSet<Parent> Parents { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder dbContextOptionsBuilder) => dbContextOptionsBuilder
            .UseSqlServer("Data Source=.\\SQLEXPRESS;Initial Catalog=testDb;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite");

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Child>()
                .HasOne(c => c.Parent)
                .WithMany(p => p!.Children)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Parent>()
                .HasOne(p => p.SpecialChild)
                .WithOne()
                .HasForeignKey<Parent>(p => p.SpecialChildId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
