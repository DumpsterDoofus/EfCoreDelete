using System.Collections.Generic;

namespace EfCoreDeleteBug
{
    public class Parent : Entity
    {
        public List<Child> Children { get; set; } = null!;

        public Child SpecialChild { get; set; } = null!;
        public int SpecialChildId { get; set; }
    }
}
