using System.Linq.Expressions;
using LabWeb.Models.Entities;
using LabWeb.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.Caching.Memory;

namespace LabWeb.Repositories.CachedRepositories;

public class CachedShoppingListRepository : IShoppingListRepository
{
    private readonly IShoppingListRepository _decorated;
    private readonly IMemoryCache _memoryCache;

    public CachedShoppingListRepository(IShoppingListRepository decorated, IMemoryCache memoryCache)
    {
        _decorated = decorated;
        _memoryCache = memoryCache;
    }

    public void Delete(ShoppingList entity)
    {
        string key = $"member-{entity.Id}";

        _decorated.Delete(entity);

        _memoryCache.Remove(key);
    }

    //not
    public void DeleteAll(IEnumerable<ShoppingList> entities)
    {
        _decorated.DeleteAll(entities);
    }

    //not
    public IQueryable<ShoppingList> GetAll(Func<IQueryable<ShoppingList>, IIncludableQueryable<ShoppingList, object>>? include = null)
    {
        return _decorated.GetAll(include);
    }

    public async Task<IEnumerable<ShoppingList>?> GetAllPaginated(int skip, int limit, Func<IQueryable<ShoppingList>, IIncludableQueryable<ShoppingList, object>>? include = null)
    {
        string key = $"member-{skip}-{limit}-list";

        return await _memoryCache.GetOrCreateAsync(key, entry =>
        {
            entry.SetAbsoluteExpiration(TimeSpan.FromMinutes(2));

            return _decorated.GetAllPaginated(skip, limit, include);
        });
    }

    //not
    public async Task<ShoppingList> GetFirstAsync(Expression<Func<ShoppingList, bool>>? predicate = null, Func<IQueryable<ShoppingList>, IIncludableQueryable<ShoppingList, object>>? include = null)
    {
        return await _decorated.GetFirstAsync(predicate, include);
    }

    public async Task<ShoppingList?> GetFirstOrDefaultAsync(Expression<Func<ShoppingList, bool>>? predicate = null, Func<IQueryable<ShoppingList>, IIncludableQueryable<ShoppingList, object>>? include = null)
    {
        return await _decorated.GetFirstOrDefaultAsync(predicate, include);
    }

    public async Task<ShoppingList?> GetByIdAsync(Guid id)
    {
        string key = $"member-{id}";

        return await _memoryCache.GetOrCreateAsync(key, entry =>
        {
            entry.SetAbsoluteExpiration(TimeSpan.FromMinutes(2));

            return _decorated.GetByIdAsync(id);
        });
    }


    //not
    public IQueryable<ShoppingList> GetWhere(Expression<Func<ShoppingList, bool>>? predicate, Func<IQueryable<ShoppingList>, IIncludableQueryable<ShoppingList, object>>? include = null, bool ignoreDbSet = false)
    {
        return _decorated.GetWhere(predicate, include, ignoreDbSet);
    }


    //not
    public void Patch(ShoppingList entity)
    {
        _decorated.Patch(entity);
    }

    public async Task Post(ShoppingList entity)
    {
        await _decorated.Post(entity);

    }

    public async Task SaveChangesAsync()
    {
        await _decorated.SaveChangesAsync();
    }

    public void Update(ShoppingList entity)
    {
        string key = $"member-{entity.Id}";

        _decorated.Update(entity);

        _memoryCache.Set(key, entity);
    }

    //not
    public async Task UpdateMany(Expression<Func<ShoppingList, bool>> predicate, Expression<Func<SetPropertyCalls<ShoppingList>, SetPropertyCalls<ShoppingList>>> setPropertyCalls)
    {
        await _decorated.UpdateMany(predicate, setPropertyCalls);
    }
}