using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace EfCoreDeleteBug
{
    public static class Program
    {
        public static void Main()
        {
            var testContext = new TestContext();
            testContext.Database.EnsureCreated();

            // Happy path
            var parentId = AddParent();
            var parent = testContext.Parents
                // Reload the parent without loading SpecialChild
                .Single(p => p.Id == parentId);
            testContext.Remove(parent);
            testContext.SaveChanges();

            // Sad path
            parentId = AddParent();
            parent = testContext.Parents
                // Reload the parent, but this time, load SpecialChild
                .Include(p => p.SpecialChild)
                .Single(p => p.Id == parentId);
            testContext.Remove(parent);
            // Exception is thrown here
            testContext.SaveChanges(); 
        }

        private static int AddParent()
        {
            var testContext = new TestContext();

            var child = new Child();
            testContext.Add(child);
            testContext.SaveChanges();

            var child2 = new Child();
            testContext.Add(child2);
            testContext.SaveChanges();

            // Reloading children from another context is needed to avoid cirular reference errors on saving parent, but that's a separate issue (https://github.com/dotnet/efcore/issues/11888#issuecomment-386395972)
            testContext.Dispose();
            testContext = new TestContext();
            child = testContext.Children.Find(child.Id);
            child2 = testContext.Children.Find(child2.Id);

            var parent = new Parent
            {
                SpecialChild = child,
                Children = new List<Child> { child, child2 }
            };
            testContext.Add(parent);
            testContext.SaveChanges();
            return parent.Id;
        }
    }
}
