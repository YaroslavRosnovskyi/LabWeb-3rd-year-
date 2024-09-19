using LabWeb.DTOs;
using LabWeb.Models;
using LabWeb.Repositories.Interfaces;
using LabWeb.Services.Interfaces;

namespace LabWeb.Services;

public class ShoppingListService : GenericService<ShoppingList, ShoppingListDto>, IShoppingListService
{
    public ShoppingListService(IShoppingListRepository repository) : base(repository)
    {
        
    }
}