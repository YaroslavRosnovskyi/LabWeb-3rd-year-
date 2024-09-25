using LabWeb.Models.Entities;
using LabWeb.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.Caching.Memory;
using System.Linq.Expressions;

namespace LabWeb.Repositories.CachedRepositories;

public class CachedItemCategoryRepository : IItemCategoryRepository
{
    private readonly IItemCategoryRepository _decorated;
    private readonly IMemoryCache _memoryCache;

    public CachedItemCategoryRepository(IItemCategoryRepository decorated, IMemoryCache memoryCache)
    {
        _decorated = decorated;
        _memoryCache = memoryCache;
    }

    public void Delete(ItemCategory entity)
    {
        string key = $"member-{entity.Id}";

        _decorated.Delete(entity);

        _memoryCache.Remove(key);
    }


    //not
    public void DeleteAll(IEnumerable<ItemCategory> entities)
    {
        _decorated.DeleteAll(entities);
    }

    //not
    public IQueryable<ItemCategory> GetAll(Func<IQueryable<ItemCategory>, IIncludableQueryable<ItemCategory, object>>? include = null)
    {
        return _decorated.GetAll(include);
    }

    public async Task<IEnumerable<ItemCategory>?> GetAllPaginated(int skip, int limit, Func<IQueryable<ItemCategory>, IIncludableQueryable<ItemCategory, object>>? include = null)
    {
        string key = $"member-{skip}-{limit}-item-category";

        return await _memoryCache.GetOrCreateAsync(key, entry =>
        {
            entry.SetAbsoluteExpiration(TimeSpan.FromMinutes(2));

            return _decorated.GetAllPaginated(skip, limit, include);
        });
    }

    //not
    public async Task<ItemCategory> GetFirstAsync(Expression<Func<ItemCategory, bool>>? predicate = null, Func<IQueryable<ItemCategory>, IIncludableQueryable<ItemCategory, object>>? include = null)
    {
        return await _decorated.GetFirstAsync(predicate, include);
    }

    public async Task<ItemCategory?> GetFirstOrDefaultAsync(Expression<Func<ItemCategory, bool>>? predicate = null, Func<IQueryable<ItemCategory>, IIncludableQueryable<ItemCategory, object>>? include = null)
    {
        return await _decorated.GetFirstOrDefaultAsync(predicate, include);
    }

    public async Task<ItemCategory?> GetByIdAsync(Guid id)
    {
        string key = $"member-{id}";

        return await _memoryCache.GetOrCreateAsync(key, entry =>
        {
            entry.SetAbsoluteExpiration(TimeSpan.FromMinutes(2));

            return _decorated.GetByIdAsync(id);
        });
    }


    //not
    public IQueryable<ItemCategory> GetWhere(Expression<Func<ItemCategory, bool>>? predicate, Func<IQueryable<ItemCategory>, IIncludableQueryable<ItemCategory, object>>? include = null, bool ignoreDbSet = false)
    {
        return _decorated.GetWhere(predicate, include, ignoreDbSet);
    }


    //not
    public void Patch(ItemCategory entity)
    {
        _decorated.Patch(entity);
    }

    public async Task Post(ItemCategory entity)
    {
        await _decorated.Post(entity);

    }

    public async Task SaveChangesAsync()
    {
        await _decorated.SaveChangesAsync();
    }

    public void Update(ItemCategory entity)
    {
        string key = $"member-{entity.Id}";

        _decorated.Update(entity);

        _memoryCache.Set(key, entity);
    }

    //not
    public async Task UpdateMany(Expression<Func<ItemCategory, bool>> predicate, Expression<Func<SetPropertyCalls<ItemCategory>, SetPropertyCalls<ItemCategory>>> setPropertyCalls)
    {
        await _decorated.UpdateMany(predicate, setPropertyCalls);
    }
}