using LabWeb.Models;
using LabWeb.Repositories.Interfaces;
using LabWeb.Services.Interfaces;

namespace LabWeb.Services;

public class ShoppingListService : GenericService<ShoppingList>, IShoppingListService
{
    public ShoppingListService(IShoppingListRepository repository) : base(repository)
    {
        
    }
}