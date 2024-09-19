using LabWeb.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace LabWeb.DTOs;

public class ShoppingListDto : BaseDto
{
    [StringLength(50)]
    public string? Name { get; set; }
    public Guid UserId { get; set; }
}