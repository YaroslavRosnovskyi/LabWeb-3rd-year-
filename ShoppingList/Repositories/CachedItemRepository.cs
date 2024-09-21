using System.Linq.Expressions;
using LabWeb.Models;
using LabWeb.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;

namespace LabWeb.Repositories;

public class CachedItemRepository : IItemRepository
{
    private readonly IItemRepository _decorated;
    private readonly IMemoryCache _memoryCache;

    public CachedItemRepository(IItemRepository decorated, IMemoryCache memoryCache)
    {
        _decorated = decorated;
        _memoryCache = memoryCache;
    }

    public void Delete(Item entity)
    {
        string key = $"member-{entity.Id}";

        _decorated.Delete(entity);

        _memoryCache.Remove(key);
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

    public async Task<IEnumerable<Item>?> GetAllPaginated(int skip, int limit, Func<IQueryable<Item>, IIncludableQueryable<Item, object>>? include = null)
    {
        string key = $"member-{skip}-{limit}-item";

        return await _memoryCache.GetOrCreateAsync(key, entry =>
        {
            entry.SetAbsoluteExpiration(TimeSpan.FromMinutes(2));

            return _decorated.GetAllPaginated(skip, limit, include);
        });
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

        return await _memoryCache.GetOrCreateAsync(key, entry =>
        {
            entry.SetAbsoluteExpiration(TimeSpan.FromMinutes(2));

            return _decorated.GetByIdAsync(id);
        });
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
        string key = $"member-{entity.Id}";

        await _decorated.Post(entity);

        _memoryCache.Set(key, entity);
    }

    public async Task SaveChangesAsync()
    {
        await _decorated.SaveChangesAsync();
    }

    public void Update(Item entity)
    {
        string key = $"member-{entity.Id}";

        _decorated.Update(entity);

        _memoryCache.Set(key, entity);
    }

    //not
    public async Task UpdateMany(Expression<Func<Item, bool>> predicate, Expression<Func<SetPropertyCalls<Item>, SetPropertyCalls<Item>>> setPropertyCalls)
    {
        await _decorated.UpdateMany(predicate, setPropertyCalls);
    }
}