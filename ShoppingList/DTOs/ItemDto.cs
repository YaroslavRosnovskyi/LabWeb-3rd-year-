using LabWeb.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace LabWeb.DTOs;

public class ItemDto : BaseDto
{
    [StringLength(50)]
    public string Name { get; set; }
    [Range(0, Int32.MaxValue)]
    public int Quantity { get; set; }
    [StringLength(200)]
    public string Notes { get; set; }
    public Guid ShoppingListId { get; set; }
}