using LabWeb.DTOs.Interfaces;
using LabWeb.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using LabWeb.DTOs.ItemDTO;

namespace LabWeb.DTOs.ShoppingListDTO;

public class ShoppingListResponse : IResponse
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public Guid UserId { get; set; }
    public ICollection<ItemResponse> Items { get; set; } = new List<ItemResponse>();
}