using LabWeb.DTOs.Interfaces;
using LabWeb.Models;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace LabWeb.DTOs.ItemDTO;

public class ItemRequest : IRequest
{
    public string Name { get; set; }
    public int Quantity { get; set; }
    public string Notes { get; set; }
    public decimal Price { get; set; }
    public Guid ShoppingListId { get; set; }
    public Guid ItemCategoryId { get; set; }
}