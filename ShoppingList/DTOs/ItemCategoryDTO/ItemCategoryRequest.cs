using LabWeb.DTOs.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace LabWeb.DTOs.ItemCategoryDTO;

public class ItemCategoryRequest : IRequest
{
    public string Name { get; set; }
}