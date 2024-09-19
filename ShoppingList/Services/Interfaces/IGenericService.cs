using LabWeb.Models;

namespace LabWeb.Services.Interfaces
{
    public interface IGenericService<TEntity> where TEntity : BaseEntity
    {
        Task DeleteAsync(TEntity entity);
        Task<TEntity?> FindByIdAsync(Guid id);
        Task<List<TEntity>> GetAllAsync();
        Task<TEntity> Insert(TEntity entity);
        Task<TEntity> Update(TEntity entity);
    }
}