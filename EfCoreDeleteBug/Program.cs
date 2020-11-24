using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace EfCoreDeleteBug
{
    public static class Program
    {
        public static void Main()
        {
            Migrate();
            HappyPath();
            SadPath();
        }

        private static void Migrate()
        {
            using var dbContext = new TestContext();
            dbContext.Database.EnsureCreated();
        }

        private static void HappyPath()
        {
            using var dbContext = new TestContext();
            var parentId = AddParent();
            var parent = dbContext.Parents
                // Reload the parent without loading SpecialChild
                .Single(p => p.Id == parentId);
            dbContext.Remove(parent);
            dbContext.SaveChanges();
        }

        private static void SadPath()
        {
            using var dbContext = new TestContext();
            var parentId = AddParent();
            var parent = dbContext.Parents
                // Reload the parent, but this time, load SpecialChild
                .Include(p => p.SpecialChild)
                .Single(p => p.Id == parentId);
            dbContext.Remove(parent);
            // Exception is thrown here
            dbContext.SaveChanges();
        }

        private static int AddParent()
        {
            var dbContext = new TestContext();

            var child = new Child();
            dbContext.Add(child);
            dbContext.SaveChanges();

            var child2 = new Child();
            dbContext.Add(child2);
            dbContext.SaveChanges();

            // Reloading children from another context is needed to avoid cirular reference errors on saving parent, but that's a separate issue (https://github.com/dotnet/efcore/issues/11888#issuecomment-386395972)
            dbContext.Dispose();
            dbContext = new TestContext();
            child = dbContext.Children.Find(child.Id);
            child2 = dbContext.Children.Find(child2.Id);

            var parent = new Parent
            {
                SpecialChild = child,
                Children = new List<Child> { child, child2 }
            };
            dbContext.Add(parent);
            dbContext.SaveChanges();
            return parent.Id;
        }
    }
}
