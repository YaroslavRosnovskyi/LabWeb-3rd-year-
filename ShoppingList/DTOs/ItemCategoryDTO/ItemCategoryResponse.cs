using LabWeb.DTOs.Interfaces;

namespace LabWeb.DTOs.ItemCategoryDTO;

public class ItemCategoryResponse : IResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; }
}