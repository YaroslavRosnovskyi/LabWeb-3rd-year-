using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LabWeb.Models;

public class ShoppingList : BaseEntity
{
    [StringLength(50)]
    public string? Name { get; set; }
    [ForeignKey(nameof(ApplicationUser))]
    public Guid UserId { get; set; }
    public ApplicationUser? ApplicationUser { get; set; }
    public ICollection<Item> Items { get; set; } = new List<Item>();
}