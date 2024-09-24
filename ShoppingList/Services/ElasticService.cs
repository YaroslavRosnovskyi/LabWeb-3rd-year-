using Elastic.Clients.Elasticsearch;
using LabWeb.Models;
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

            // Check if the index does not exist, then create it
            if (!indexExistsResponse.Exists)
            {
                var createIndexResponse = await _client.Indices.CreateAsync(indexName);

                if (!createIndexResponse.IsSuccess())
                {
                    throw new Exception($"Failed to create index {indexName}: {createIndexResponse.DebugInformation}");
                }
            }
        }

        public async Task<bool> AddOrUpdate(Item item)
        {
            var response = await _client.IndexAsync(item, idx => idx.Index(_elasticSettings.DefaultIndex)
                .OpType(OpType.Index));

            return response.IsValidResponse;
        }


        public async Task<bool> AddOrUpdateBulk(IEnumerable<Item> items, string indexName)
        {
            var response = await _client.BulkAsync(b => b.Index(_elasticSettings.DefaultIndex)
                .UpdateMany(items, (ud, u) => ud.Doc(u).DocAsUpsert()));

            return response.IsValidResponse;
        }

        public async Task<Item> Get(string key)
        {
            var response = await _client.GetAsync<Item>(key, g => g.Index(_elasticSettings.DefaultIndex));

            return response.Source;
        }


        public async Task<List<Item>> GetAll(string key)
        {
            var response = await _client.SearchAsync<Item>(key, g => g.Index(_elasticSettings.DefaultIndex));

            return response.IsValidResponse ? response.Documents.ToList() : new List<Item>();
        }


        public async Task<bool> Remove(string key)
        {
            var response = await _client.DeleteAsync<Item>(key, g => g.Index(_elasticSettings.DefaultIndex));

            return response.IsValidResponse;
        }

        public async Task<long?> RemoveAll()
        {
            var response = await _client.DeleteByQueryAsync<Item>(g => g.Indices(_elasticSettings.DefaultIndex));

            return response.IsValidResponse ? response.Deleted : default;
        }
    }

    public class ElasticSettings  
    {
        public string Url { get; set; }
        public string DefaultIndex { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
