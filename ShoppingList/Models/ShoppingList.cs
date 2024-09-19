using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LabWeb.Models;

public class ShoppingList : BaseEntity
{
    [StringLength(50)]
    public string? Name { get; set; }
    [ForeignKey(nameof(User))]
    public Guid UserId { get; set; }
    public User? User { get; set; }
    public ICollection<Item> Items { get; set; } = new List<Item>();
}