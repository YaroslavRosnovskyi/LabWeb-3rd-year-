using LabWeb.Models;
using LabWeb.Repositories.Interfaces;
using LabWeb.Services.Interfaces;

namespace LabWeb.Services;

public class ItemService : GenericService<Item>, IItemService
{
    public ItemService(IItemRepository itemRepository) : base(itemRepository)
    {
        
    }

}