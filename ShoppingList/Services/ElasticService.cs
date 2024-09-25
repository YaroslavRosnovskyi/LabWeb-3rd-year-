using Elastic.Clients.Elasticsearch;
using LabWeb.DTOs;
using LabWeb.DTOs.ItemDTO;
using LabWeb.Models;
using LabWeb.Services.Interfaces;
using LabWeb.SettingOptions;
using Microsoft.Extensions.Options;

namespace LabWeb.Services
{
    public class ElasticService : IElasticService
    {
        private readonly ElasticsearchClient _client;
        private readonly ElasticSettings _elasticSettings;

        public ElasticService(IOptions<ElasticSettings> elasticSettings)
        {
            _elasticSettings = elasticSettings.Value;

            var settings = new ElasticsearchClientSettings(new Uri(_elasticSettings.Url))
                .DefaultIndex(_elasticSettings.DefaultIndex);

            _client = new ElasticsearchClient(settings);
        }

        public async Task CreateIndexIfNotExists(string indexName)
        {
            var indexExistsResponse = await _client.Indices.ExistsAsync(indexName);

            if (!indexExistsResponse.Exists)
            {
                var createIndexResponse = await _client.Indices.CreateAsync(indexName);

                if (!createIndexResponse.IsSuccess())
                {
                    throw new Exception($"Failed to create index {indexName}: {createIndexResponse.DebugInformation}");
                }
            }
        }

        public async Task DeleteIndexIfExists(string indexName)
        {
            var indexExistsResponse = await _client.Indices.ExistsAsync(indexName);

            if (indexExistsResponse.Exists)
            {
                var createIndexResponse = await _client.Indices.DeleteAsync(indexName);

                if (!createIndexResponse.IsSuccess())
                {
                    throw new Exception($"Failed to create index {indexName}: {createIndexResponse.DebugInformation}");
                }
            }
        }

        public async Task<bool> AddOrUpdate(ItemResponse item)
        {
            var response = await _client.IndexAsync(item, idx => idx.Index(_elasticSettings.DefaultIndex)
                .OpType(OpType.Index));

            return response.IsValidResponse;
        }


        public async Task<bool> AddOrUpdateBulk(IEnumerable<ItemResponse> items, string indexName)
        {
            var response = await _client.BulkAsync(b => b.Index(_elasticSettings.DefaultIndex)
                .UpdateMany(items, (ud, u) => ud.Doc(u).DocAsUpsert()));

            return response.IsValidResponse;
        }

        public async Task<ItemResponse> Get(string key)
        {
            var response = await _client.GetAsync<ItemResponse>(key, g => g.Index(_elasticSettings.DefaultIndex));

            return response.Source;
        }


        public async Task<List<ItemResponse>> GetAll(string key)
        {
            var response = await _client.SearchAsync<ItemResponse>(key, g => g.Index(_elasticSettings.DefaultIndex));

            return response.IsValidResponse ? response.Documents.ToList() : new List<ItemResponse>();
        }


        public async Task<bool> Remove(string key)
        {
            var response = await _client.DeleteAsync<ItemResponse>(key, g => g.Index(_elasticSettings.DefaultIndex));

            return response.IsValidResponse;
        }

        public async Task<long?> RemoveAll()
        {
            var response = await _client.DeleteByQueryAsync<ItemResponse>(g => g.Indices(_elasticSettings.DefaultIndex));

            return response.IsValidResponse ? response.Deleted : default;
        }

        public async Task<List<ItemResponse>> Search(string query, int skip = 0, int limit = 10)
        {

            var searchResponse = await _client.SearchAsync<ItemResponse>(s => s
                .Index(_elasticSettings.DefaultIndex)
                .Query(q => q
                    .MultiMatch(m => m
                        .Query(query)
                        .Fields("name, notes, categoryName")
                    )
                )

            );

            if (searchResponse.IsValidResponse)
            {
                return searchResponse.Documents.ToList();
            }

            throw new Exception($"Failed to search for {query}: {searchResponse.DebugInformation}");
        }
    }
}
