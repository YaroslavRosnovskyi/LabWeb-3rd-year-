using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using LabWeb.Models.IdentityModels;

namespace LabWeb.Models.Entities;

public class ShoppingList : BaseEntity
{
    [StringLength(50)]
    public string? Name { get; set; }
    [ForeignKey(nameof(ApplicationUser))]
    public Guid UserId { get; set; }
    public ApplicationUser? ApplicationUser { get; set; }
    public ICollection<Item> Items { get; set; } = new List<Item>();
}