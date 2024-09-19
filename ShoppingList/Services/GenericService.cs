using LabWeb.Models;
using LabWeb.Repositories.Interfaces;
using LabWeb.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LabWeb.Services
{
    public abstract class GenericService<TEntity> 
        : IGenericService<TEntity> 
        where TEntity : BaseEntity
    {
        protected readonly IGenericRepository<TEntity> repository;

        protected GenericService(IGenericRepository<TEntity> repository)
        {
            this.repository = repository;
        }

        public virtual async Task<List<TEntity>> GetAllAsync()
        {
            return await repository.GetAll().ToListAsync();
        }

        public virtual async Task<TEntity?> FindByIdAsync(Guid id)
        {
            return await repository.GetFirstOrDefaultAsync(entity => entity.Id == id);
        }

        public virtual async Task<TEntity> Insert(TEntity entity)
        {
            await repository.Post(entity);
            await repository.SaveChangesAsync();
            return entity;
        }

        public virtual async Task<TEntity> Update(TEntity entity)
        {
            repository.Update(entity);
            await repository.SaveChangesAsync();

            var updatedEntity = await repository.GetFirstOrDefaultAsync(e => e.Id == entity.Id);

            return updatedEntity;
        }

        public virtual async Task DeleteAsync(TEntity entity)
        {
            repository.Delete(entity);
            await repository.SaveChangesAsync();
        }
    }
}
