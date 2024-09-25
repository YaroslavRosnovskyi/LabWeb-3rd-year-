using LabWeb.DTOs;
using LabWeb.DTOs.ShoppingListDTO;
using LabWeb.Models;

namespace LabWeb.Services.Interfaces;

public interface IShoppingListService : IGenericService<ShoppingListRequest, ShoppingListResponse>
{
    List<ShoppingListResponse> GetShoppingListByUserId(Guid userId);
}