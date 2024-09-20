using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace LabWeb.Models;

public class User : BaseEntity
{
    [StringLength(50)]
    public string? Name { get; set; }
    [EmailAddress]
    public string? Email { get; set; }
    public string? PasswordHash { get; set; }
    public string? ImageName { get; set; }
    public ICollection<ShoppingList> ShoppingList { get; set; } = new List<ShoppingList>();
}