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
            SadPathThatThrows();
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
                .Single(p => p.Id == parentId);
            dbContext.Remove(parent);
            dbContext.SaveChanges();
        }

        private static void SadPathThatThrows()
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

            var child1 = new Child();
            var child2 = new Child();
            dbContext.Children.AddRange(child1, child2);
            dbContext.SaveChanges();

            // Saving in two steps is needed to avoid cirular reference errors on saving parent, but that's a separate issue: https://github.com/dotnet/efcore/issues/11888#issuecomment-386395972

            var parent = new Parent
            {
                Children = new List<Child> { child1, child2 },
                SpecialChild = child1
            };
            dbContext.Parents.Add(parent);
            dbContext.SaveChanges();

            return parent.Id;
        }
    }
}
