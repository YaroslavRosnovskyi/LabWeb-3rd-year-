using LabWeb.Models;

namespace LabWeb.Services
{
    public interface IElasticService
    {
        Task<bool> AddOrUpdate(Item item);
        Task<bool> AddOrUpdateBulk(IEnumerable<Item> items, string indexName);
        Task CreateIndexIfNotExists(string indexName);
        Task<Item> Get(string key);
        Task<List<Item>> GetAll(string key);
        Task<bool> Remove(string key);
        Task<long?> RemoveAll();
    }
}