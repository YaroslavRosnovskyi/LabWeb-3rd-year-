using LabWeb.DTOs;
using LabWeb.Models;

namespace LabWeb.Services.Interfaces
{
    public interface IGenericService<TEntity, TMappedEntity> 
        where TEntity : BaseEntity
        where TMappedEntity : BaseDto
    {
        Task DeleteAsync(TMappedEntity entity);
        Task<TMappedEntity?> FindByIdAsync(Guid id);
        Task<List<TMappedEntity>> GetAllAsync();
        Task<TMappedEntity> Insert(TMappedEntity entity);
        Task<TMappedEntity> Update(TMappedEntity entity);
    }
}