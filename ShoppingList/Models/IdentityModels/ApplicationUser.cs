using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using LabWeb.Models.Entities;
using Microsoft.AspNetCore.Identity;

namespace LabWeb.Models.IdentityModels;

public class ApplicationUser : IdentityUser<Guid>
{
    public string? ImageName { get; set; } = "Default.jpg";
    public ICollection<ShoppingList> ShoppingList { get; set; } = new List<ShoppingList>();
}