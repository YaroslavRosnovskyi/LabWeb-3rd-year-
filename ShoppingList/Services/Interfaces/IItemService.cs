using LabWeb.DTOs;
using LabWeb.Models;

namespace LabWeb.Services.Interfaces;

public interface IItemService : IGenericService<Item, ItemDto>
{
    Task<List<ItemDto>> SearchAsync(string query);
    Task CreateIndexIfNotExistsAsync(string indexName);
}