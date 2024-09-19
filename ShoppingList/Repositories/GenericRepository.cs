using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using LabWeb.Context;
using LabWeb.Models;
using Microsoft.EntityFrameworkCore.Query;
using LabWeb.Repositories.Interfaces;

namespace LabWeb.Repositories;

public abstract class GenericRepository<TEntity> : IGenericRepository<TEntity> where TEntity : BaseEntity
{
    private readonly ApplicationDbContext _context;
    protected readonly DbSet<TEntity> dbSet;

    protected virtual IQueryable<TEntity> PrepareDbSet()
    {
        return dbSet;
    }

    public GenericRepository(ApplicationDbContext dataContext)
    {
        _context = dataContext ?? throw new ArgumentNullException();
        dbSet = dataContext.Set<TEntity>();
    }

    public virtual async Task<TEntity?> GetFirstOrDefaultAsync(Expression<Func<TEntity, bool>>? predicate = null,
        Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null)
    {
        var preparedDbSet = PrepareDbSet();

        if (include != null)
        {
            preparedDbSet = include(preparedDbSet);
        }

        if (predicate is null)
            return await preparedDbSet.FirstOrDefaultAsync();

        return await preparedDbSet.FirstOrDefaultAsync(predicate);
    }

    public virtual async Task<TEntity> GetFirstAsync(Expression<Func<TEntity, bool>>? predicate = null,
        Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null)
    {
        var preparedDbSet = PrepareDbSet();

        if (include != null)
        {
            preparedDbSet = include(preparedDbSet);
        }

        if (predicate is null)
            return await preparedDbSet.FirstAsync();

        return await preparedDbSet.FirstAsync(predicate);
    }

    public virtual IQueryable<TEntity> GetWhere(Expression<Func<TEntity, bool>>? predicate,
        Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null,
        bool ignoreDbSet = false)
    {
        if (predicate is null)
            throw new ArgumentNullException(nameof(predicate));

        IQueryable<TEntity> preparedDbSet = _context.Set<TEntity>();
        if (!ignoreDbSet)
            preparedDbSet = PrepareDbSet();

        if (include != null)
        {
            preparedDbSet = include(preparedDbSet);
        }

        return preparedDbSet.Where(predicate);
    }

    public virtual IQueryable<TEntity> GetAll(
        Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null)
    {
        var preparedDbSet = PrepareDbSet();

        if (include != null)
        {
            preparedDbSet = include(preparedDbSet);
        }

        return preparedDbSet;
    }

    public virtual IQueryable<TEntity> GetAllPaginated(int skip, int limit,
        Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null)
    {
        var preparedDbSet = PrepareDbSet();

        if (include != null)
        {
            preparedDbSet = include(preparedDbSet);
        }

        var paginatedPreparedDbSet = preparedDbSet.Skip(skip).Take(limit);

        return paginatedPreparedDbSet;
    }

    public async Task Post(TEntity entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        await dbSet.AddAsync(entity);
    }

    public virtual void Update(TEntity entity)
    {
        if (entity is null)
            throw new ArgumentNullException(nameof(entity));

        dbSet.Update(entity);
    }

    public virtual async Task UpdateMany(Expression<Func<TEntity, bool>> predicate,
        Expression<Func<SetPropertyCalls<TEntity>, SetPropertyCalls<TEntity>>> setPropertyCalls)
    {
        var preparedDbSet = PrepareDbSet();

        await preparedDbSet.Where(predicate).ExecuteUpdateAsync(setPropertyCalls);
    }

    public virtual void Delete(TEntity entity)
    {
        if (entity is null)
            throw new ArgumentNullException(nameof(entity));

        var entry = _context.Entry(entity);
        entry.State = EntityState.Deleted;
        dbSet.Remove(entity);
    }

    public virtual void DeleteAll(IEnumerable<TEntity> entities)
    {
        if (entities is null)
            throw new ArgumentNullException(nameof(entities));

        foreach (var entity in entities)
        {
            var entry = _context.Entry<TEntity>(entity);
            entry.State = EntityState.Deleted;
            dbSet.Remove(entity);
        }
    }

    public virtual void Patch(TEntity entity)
    {
        if (entity is null)
            throw new ArgumentNullException(nameof(entity));

        dbSet.Update(entity);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}