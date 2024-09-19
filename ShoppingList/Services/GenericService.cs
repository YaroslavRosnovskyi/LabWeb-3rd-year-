using System.Runtime.InteropServices.ComTypes;
using LabWeb.DTOs;
using LabWeb.Models;
using LabWeb.Repositories.Interfaces;
using LabWeb.Services.Interfaces;
using Mapster;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Microsoft.EntityFrameworkCore;

namespace LabWeb.Services
{
    public abstract class GenericService<TEntity, TMappedEntity> 
        : IGenericService<TEntity, TMappedEntity> 
        where TEntity : BaseEntity
        where TMappedEntity : BaseDto
    {
        protected readonly IGenericRepository<TEntity> repository;

        protected GenericService(IGenericRepository<TEntity> repository)
        {
            this.repository = repository;
        }

        public virtual async Task<List<TMappedEntity>> GetAllAsync()
        {
            var entities = await repository.GetAll().ToListAsync();
            var mappedEntities = entities.Adapt<List<TMappedEntity>>();
            return mappedEntities;
        }

        public virtual async Task<PaginatedResponse<TMappedEntity>> GetAllPaginatedAsync(int skip, int limit)
        {
            var entities = await repository.GetAllPaginated(skip, limit).ToListAsync();
            var mappedEntities = entities.Adapt<List<TMappedEntity>>();

            var paginatedResponse = new PaginatedResponse<TMappedEntity>
            {
                MappedEntities = mappedEntities,
                TotalCount = mappedEntities.Count,
                Limit = limit,
                Skip = skip,
            };

            return paginatedResponse;
        }

        public virtual async Task<TMappedEntity?> FindByIdAsync(Guid id)
        {
            var entity = await repository.GetFirstOrDefaultAsync(entity => entity.Id == id);
            var mappedEntity = entity.Adapt<TMappedEntity>();
            return mappedEntity;
        }

        public virtual async Task<TMappedEntity> Insert(TMappedEntity entityDto)
        {
            var entity = entityDto.Adapt<TEntity>();
            await repository.Post(entity);
            await repository.SaveChangesAsync();

            return entity.Adapt<TMappedEntity>();
        }

        public virtual async Task<TMappedEntity> Update(TMappedEntity entityDto)
        {
            var entity = entityDto.Adapt<TEntity>();
            repository.Update(entity);
            await repository.SaveChangesAsync();
            return entityDto;
        }

        public virtual async Task DeleteAsync(TMappedEntity entityDto)
        {
            var entity = entityDto.Adapt<TEntity>();
            repository.Delete(entity);
            await repository.SaveChangesAsync();
        }
    }
}
