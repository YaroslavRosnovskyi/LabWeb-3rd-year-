using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LabWeb.Models;

public class Item : BaseEntity
{
    [StringLength(50)]
    public string Name { get; set; }
    [Range(0, Int32.MaxValue)]
    public int Quantity { get; set; } 
    [StringLength(200)]
    public string Notes { get; set; } 


    [ForeignKey(nameof(ShoppingList))]
    public Guid ShoppingListId { get; set; } 
    public ShoppingList? ShoppingList { get; set; }
}