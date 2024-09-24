using LabWeb.DTOs.Interfaces;
using LabWeb.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace LabWeb.DTOs.ShoppingListDTO;

public class ShoppingListRequest : IRequest
{
    public string? Name { get; set; }
    public Guid UserId { get; set; }
}