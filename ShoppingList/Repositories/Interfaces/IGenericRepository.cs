using LabWeb.Models;
using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;

namespace LabWeb.Repositories.Interfaces
{
    public interface IGenericRepository<TEntity> where TEntity : BaseEntity
    {
        void Delete(TEntity entity);
        void DeleteAll(IEnumerable<TEntity> entities);
        IQueryable<TEntity> GetAll(Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null);
        Task<TEntity> GetFirstAsync(Expression<Func<TEntity, bool>>? predicate = null, Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null);
        Task<TEntity?> GetFirstOrDefaultAsync(Expression<Func<TEntity, bool>>? predicate = null, Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null);
        IQueryable<TEntity> GetWhere(Expression<Func<TEntity, bool>>? predicate, Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null, bool ignoreDbSet = false);
        void Patch(TEntity entity);
        Task Post(TEntity entity);
        Task SaveChangesAsync();
        void Update(TEntity entity);
        Task UpdateMany(Expression<Func<TEntity, bool>> predicate, Expression<Func<SetPropertyCalls<TEntity>, SetPropertyCalls<TEntity>>> setPropertyCalls);
    }
}