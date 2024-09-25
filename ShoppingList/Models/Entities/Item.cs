using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LabWeb.Models.Entities;

public class Item : BaseEntity
{
    [StringLength(50)]
    public string Name { get; set; }
    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }
    [StringLength(200)]
    public string Notes { get; set; }
    [Precision(14, 2)]
    [Range(0.99, double.MaxValue)]
    public decimal Price { get; set; }
    [ForeignKey(nameof(ShoppingList))]
    public Guid ShoppingListId { get; set; }
    public ShoppingList? ShoppingList { get; set; }
    [ForeignKey(nameof(Category))]
    public Guid ItemCategoryId { get; set; }
    public ItemCategory? Category { get; set; }
}