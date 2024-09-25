using LabWeb.DTOs.ItemCategoryDTO;
using LabWeb.DTOs.ItemDTO;
using LabWeb.Models.Entities;
using LabWeb.Repositories.Interfaces;
using LabWeb.Services.Interfaces;

namespace LabWeb.Services;

public class ItemCategoryService : GenericService<ItemCategory, ItemCategoryRequest, ItemCategoryResponse>, IItemCategoryService
{
    public ItemCategoryService(IItemCategoryRepository itemCategoryRepository) : base(itemCategoryRepository)
    {
        
    }
}