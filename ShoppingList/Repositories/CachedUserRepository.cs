using LabWeb.Models;
using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;
using LabWeb.Repositories.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace LabWeb.Repositories;

public class CachedUserRepository : IUserRepository
{
    private readonly IUserRepository _decorated;
    private readonly IMemoryCache _memoryCache;

    public CachedUserRepository(IUserRepository decorated, IMemoryCache memoryCache)
    {
        _decorated = decorated;
        _memoryCache = memoryCache;
    }

    public void Delete(User entity)
    {
        string key = $"member-{entity.Id}";

        _decorated.Delete(entity);

        _memoryCache.Remove(key);
    }

    //not
    public void DeleteAll(IEnumerable<User> entities)
    {
        _decorated.DeleteAll(entities);
    }

    //not
    public IQueryable<User> GetAll(Func<IQueryable<User>, IIncludableQueryable<User, object>>? include = null)
    {
        return _decorated.GetAll(include);
    }

    public async Task<IEnumerable<User>?> GetAllPaginated(int skip, int limit, Func<IQueryable<User>, IIncludableQueryable<User, object>>? include = null)
    {
        string key = $"member-{skip}-{limit}-user";

        return await _memoryCache.GetOrCreateAsync(key, entry =>
        {
            entry.SetAbsoluteExpiration(TimeSpan.FromMinutes(2));

            return _decorated.GetAllPaginated(skip, limit, include);
        });
    }

    //not
    public async Task<User> GetFirstAsync(Expression<Func<User, bool>>? predicate = null, Func<IQueryable<User>, IIncludableQueryable<User, object>>? include = null)
    {
        return await _decorated.GetFirstAsync(predicate, include);
    }

    public async Task<User?> GetFirstOrDefaultAsync(Expression<Func<User, bool>>? predicate = null, Func<IQueryable<User>, IIncludableQueryable<User, object>>? include = null)
    {
        return await _decorated.GetFirstOrDefaultAsync(predicate, include);
    }

    public async Task<User?> GetByIdAsync(Guid id)
    {
        string key = $"member-{id}";

        return await _memoryCache.GetOrCreateAsync(key, entry =>
        {
            entry.SetAbsoluteExpiration(TimeSpan.FromMinutes(2));

            return _decorated.GetByIdAsync(id);
        });
    }


    //not
    public IQueryable<User> GetWhere(Expression<Func<User, bool>>? predicate, Func<IQueryable<User>, IIncludableQueryable<User, object>>? include = null, bool ignoreDbSet = false)
    {
        return _decorated.GetWhere(predicate, include, ignoreDbSet);
    }


    //not
    public void Patch(User entity)
    {
        _decorated.Patch(entity);
    }

    public async Task Post(User entity)
    {
        string key = $"member-{entity.Id}";

        await _decorated.Post(entity);

        _memoryCache.Set(key, entity);
    }

    public async Task SaveChangesAsync()
    {
        await _decorated.SaveChangesAsync();
    }

    public void Update(User entity)
    {
        string key = $"member-{entity.Id}";

        _decorated.Update(entity);

        _memoryCache.Set(key, entity);
    }

    //not
    public async Task UpdateMany(Expression<Func<User, bool>> predicate, Expression<Func<SetPropertyCalls<User>, SetPropertyCalls<User>>> setPropertyCalls)
    {
        await _decorated.UpdateMany(predicate, setPropertyCalls);
    }
}