using System.ComponentModel.DataAnnotations;

namespace EfCoreDeleteBug
{
public class Entity
{
    [Key]
    public int Id { get; set; }
}
}
