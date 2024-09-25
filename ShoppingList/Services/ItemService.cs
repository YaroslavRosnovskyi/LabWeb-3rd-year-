using Elastic.Clients.Elasticsearch;
using LabWeb.DTOs;
using LabWeb.DTOs.ItemDTO;
using LabWeb.Migrations;
using LabWeb.Models;
using LabWeb.Repositories.Interfaces;
using LabWeb.Services.Interfaces;
using LabWeb.SettingOptions;
using Mapster;
using Microsoft.Extensions.Options;

namespace LabWeb.Services;

public class ItemService : GenericService<Item, ItemRequest, ItemResponse>, IItemService
{

    public ItemService(IItemRepository itemRepository) : base(itemRepository)
    {
        
    }


    

}