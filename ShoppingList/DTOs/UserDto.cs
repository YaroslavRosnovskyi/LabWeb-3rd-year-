using LabWeb.Models;
using System.ComponentModel.DataAnnotations;

namespace LabWeb.DTOs;

public class UserDto : BaseDto
{
    [StringLength(50)]
    public string? Name { get; set; }
    [EmailAddress]
    public string? Email { get; set; }
    public string? PasswordHash { get; set; }
    public string? ImageName { get; set; } = "Default.jpg";
}