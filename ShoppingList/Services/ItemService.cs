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
    private readonly ElasticsearchClient _elasticsearchClient;
    private readonly ElasticSettings _elasticSettings;

    public ItemService(IItemRepository itemRepository, IOptions<ElasticSettings> elasticSettings) : base(itemRepository)
    {
        _elasticSettings = elasticSettings.Value;

        var settings = new ElasticsearchClientSettings(new Uri(_elasticSettings.Url))
            .DefaultIndex(_elasticSettings.DefaultIndex);

        _elasticsearchClient = new ElasticsearchClient(settings);
    }

    public virtual async Task<List<ItemDto>> SearchAsync(string query)
    {
        var searchResponse = await _elasticsearchClient.SearchAsync<Item>(s => s
            .Query(q => q
                .QueryString(qs => qs
                    .Query(query)
                )
            )
        );

        var entities = searchResponse.Documents.ToList();
        var mappedEntities = entities.Adapt<List<ItemDto>>();
        return mappedEntities;
    }

    public async Task CreateIndexIfNotExistsAsync(string indexName)
    {
        if (!_elasticsearchClient.Indices.Exists(indexName).Exists)
        {
            await _elasticsearchClient.Indices.CreateAsync(indexName);
        }
    }

    public override async Task<ItemDto> Insert(ItemDto entityDto)
    {
        var entity = entityDto.Adapt<Item>();
        await repository.Post(entity);
        await repository.SaveChangesAsync();

        await CreateIndexIfNotExistsAsync(_elasticSettings.DefaultIndex);

        // Index entity to Elasticsearch
        await _elasticsearchClient.IndexAsync(entityDto, idx =>
        {
            idx.Index(_elasticSettings.DefaultIndex)
                .OpType(OpType.Index);
        });

        return entity.Adapt<ItemDto>();
    }

    public override async Task<ItemDto> Update(ItemDto entityDto)
    {
        var entity = entityDto.Adapt<Item>();
        repository.Update(entity);
        await repository.SaveChangesAsync();

        // Update Elasticsearch index
        await _elasticsearchClient.IndexAsync(entityDto, idx =>
        {
            idx.Index(_elasticSettings.DefaultIndex)
                .OpType(OpType.Index);
        });

        return entityDto;
    }

}