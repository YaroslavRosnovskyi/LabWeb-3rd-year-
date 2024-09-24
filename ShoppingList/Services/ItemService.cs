using Elastic.Clients.Elasticsearch;
using LabWeb.DTOs;
using LabWeb.Migrations;
using LabWeb.Models;
using LabWeb.Repositories.Interfaces;
using LabWeb.Services.Interfaces;
using LabWeb.SettingOptions;
using Mapster;
using Microsoft.Extensions.Options;

namespace LabWeb.Services;

public class ItemService : GenericService<Item, ItemDto>, IItemService
{

    public ItemService(IItemRepository itemRepository) : base(itemRepository)
    {
        
    }


}