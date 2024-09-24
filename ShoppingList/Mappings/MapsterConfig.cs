using LabWeb.DTOs.ItemDTO;
using LabWeb.Models;
using Mapster;

namespace LabWeb.Mappings;

public static class MapsterConfig
{
    public static void Configure()
    {
        TypeAdapterConfig<Item, ItemResponse>.NewConfig()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.Name, src => src.Name)
            .Map(dest => dest.Notes, src => src.Notes)
            .Map(dest => dest.ShoppingListId, src => src.ShoppingListId)
            .Map(dest => dest.Quantity, src => src.Quantity)
            .Map(dest => dest.Price, src => src.Price)
            .Map(dest => dest.CategoryName, src => src.Category.Name);

    }
}