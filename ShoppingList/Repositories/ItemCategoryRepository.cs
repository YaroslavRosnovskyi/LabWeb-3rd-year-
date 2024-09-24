using LabWeb.Context;
using LabWeb.Models;
using LabWeb.Repositories.Interfaces;

namespace LabWeb.Repositories;

public class ItemCategoryRepository : GenericRepository<ItemCategory>, IItemCategoryRepository
{
    public ItemCategoryRepository(ApplicationDbContext context) : base(context)
    {
        
    }
}