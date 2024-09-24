using LabWeb.DTOs;
using LabWeb.DTOs.ShoppingListDTO;
using LabWeb.Models;
using LabWeb.Repositories.Interfaces;
using LabWeb.Services.Interfaces;

namespace LabWeb.Services;

public class ShoppingListService : GenericService<ShoppingList, ShoppingListRequest, ShoppingListResponse>, IShoppingListService
{
    public ShoppingListService(IShoppingListRepository repository) : base(repository)
    {
        
    }
}