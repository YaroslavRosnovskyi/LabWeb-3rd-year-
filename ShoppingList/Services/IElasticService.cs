using LabWeb.DTOs.ItemDTO;

namespace LabWeb.Services
{
    public interface IElasticService
    {
        Task<bool> AddOrUpdate(ItemResponse item);
        Task<bool> AddOrUpdateBulk(IEnumerable<ItemResponse> items, string indexName);
        Task CreateIndexIfNotExists(string indexName);
        Task<ItemResponse> Get(string key);
        Task<List<ItemResponse>> GetAll(string key);
        Task<bool> Remove(string key);
        Task<long?> RemoveAll();
        Task<List<ItemResponse>> Search(string query, int skip = 0, int limit = 10);
        Task DeleteIndexIfExists(string indexName);
    }
}