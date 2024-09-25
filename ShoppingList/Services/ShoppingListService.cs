using LabWeb.DTOs;
using LabWeb.DTOs.ItemDTO;
using LabWeb.DTOs.ShoppingListDTO;
using LabWeb.Models.Entities;
using LabWeb.Repositories.Interfaces;
using LabWeb.Services.Interfaces;
using Mapster;

namespace LabWeb.Services;

public class ShoppingListService : GenericService<ShoppingList, ShoppingListRequest, ShoppingListResponse>, IShoppingListService
{
    public ShoppingListService(IShoppingListRepository repository) : base(repository)
    {
        
    }

    public List<ShoppingListResponse> GetShoppingListByUserId(Guid userId)
    {
        var entities = repository.GetWhere(sl => sl.UserId == userId);

        return entities.Adapt<List<ShoppingListResponse>>();
    }
}