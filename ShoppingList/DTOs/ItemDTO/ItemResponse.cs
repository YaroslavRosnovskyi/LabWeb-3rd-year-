using LabWeb.DTOs.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace LabWeb.DTOs.ItemDTO;

public class ItemResponse : IResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public int Quantity { get; set; }
    public string Notes { get; set; }
    public decimal Price { get; set; }
    public string CategoryName { get; set; }
    public Guid ShoppingListId { get; set; }
}