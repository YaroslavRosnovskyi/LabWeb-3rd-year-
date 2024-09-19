using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LabWeb.Models;

public class ShoppingList : BaseEntity
{
    [StringLength(50)]
    public string? Name { get; set; } // Назва списку
    [ForeignKey(nameof(User))]
    public Guid UserId { get; set; } // Ідентифікатор користувача (для зв'язку з User)
    public User? User { get; set; } // Зв'язок з User
    public ICollection<Item> Items { get; set; } = new List<Item>(); // Список товарів у цьому списку
}