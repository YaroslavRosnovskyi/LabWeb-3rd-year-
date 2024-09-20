using System.Linq.Expressions;
using LabWeb.Models;
using LabWeb.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;

namespace LabWeb.Repositories;

public class CachedItemRepository : IItemRepository
{
    private readonly IItemRepository _decorated;
    private readonly IDistributedCache _distributedCache;

    public CachedItemRepository(IItemRepository decorated, IDistributedCache distributedCache)
    {
        _decorated = decorated;
        _distributedCache = distributedCache;
    }

    public void Delete(Item entity)
    {
        _decorated.Delete(entity);
    }


    //not
    public void DeleteAll(IEnumerable<Item> entities)
    {
        _decorated.DeleteAll(entities); 
    }

    //not
    public IQueryable<Item> GetAll(Func<IQueryable<Item>, IIncludableQueryable<Item, object>>? include = null)
    {
        return _decorated.GetAll(include);
    }

    public async Task<IEnumerable<Item>> GetAllPaginated(int skip, int limit, Func<IQueryable<Item>, IIncludableQueryable<Item, object>>? include = null)
    {
        return await _decorated.GetAllPaginated(skip, limit, include);
    }

    //not
    public async Task<Item> GetFirstAsync(Expression<Func<Item, bool>>? predicate = null, Func<IQueryable<Item>, IIncludableQueryable<Item, object>>? include = null)
    {
        return await _decorated.GetFirstAsync(predicate, include);
    }

    public async Task<Item?> GetFirstOrDefaultAsync(Expression<Func<Item, bool>>? predicate = null, Func<IQueryable<Item>, IIncludableQueryable<Item, object>>? include = null)
    {

        return await _decorated.GetFirstOrDefaultAsync(predicate, include);
    }

    public async Task<Item?> GetByIdAsync(Guid id)
    {
        string key = $"member-{id}";

        string? cachedMember = await _distributedCache.GetStringAsync(key);

        Item? item;
        if (string.IsNullOrEmpty(cachedMember))
        {
            item = await _decorated.GetByIdAsync(id);

            if (item is null)
            {
                return item;
            }

            await _distributedCache.SetStringAsync(key, JsonConvert.SerializeObject(item));

            return item;
        }

        item = JsonConvert.DeserializeObject<Item>(cachedMember);

        return item;
    }


    //not
    public IQueryable<Item> GetWhere(Expression<Func<Item, bool>>? predicate, Func<IQueryable<Item>, IIncludableQueryable<Item, object>>? include = null, bool ignoreDbSet = false)
    {
        return _decorated.GetWhere(predicate, include, ignoreDbSet);
    }


    //not
    public void Patch(Item entity)
    {
        _decorated.Patch(entity);
    }

    public async Task Post(Item entity)
    {
        await _decorated.Post(entity);
    }

    public async Task SaveChangesAsync()
    {
        await _decorated.SaveChangesAsync();
    }

    public void Update(Item entity)
    {
        _decorated.Update(entity);
    }

    //not
    public async Task UpdateMany(Expression<Func<Item, bool>> predicate, Expression<Func<SetPropertyCalls<Item>, SetPropertyCalls<Item>>> setPropertyCalls)
    {
        await _decorated.UpdateMany(predicate, setPropertyCalls);
    }
}