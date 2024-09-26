using System.Linq.Expressions;
using System.Runtime.InteropServices.ComTypes;
using LabWeb.DTOs;
using LabWeb.DTOs.Interfaces;
using LabWeb.Models;
using LabWeb.Repositories.Interfaces;
using LabWeb.Services.Interfaces;
using Mapster;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Microsoft.EntityFrameworkCore;

namespace LabWeb.Services
{
    public abstract class GenericService<TEntity, TRequest, TResponse> : IGenericService<TRequest, TResponse> 
        where TEntity : BaseEntity
        where TRequest : IRequest
        where TResponse : IResponse
    {
        protected readonly IGenericRepository<TEntity> repository;

        protected GenericService(IGenericRepository<TEntity> repository)
        {
            this.repository = repository;
        }

        public virtual async Task<List<TResponse>> GetAllAsync()
        {
            var entities = await repository.GetAll().ToListAsync();
            var mappedEntities = entities.Adapt<List<TResponse>>();
            return mappedEntities;
        }

        public virtual async Task<PaginatedResponse<TResponse>> GetAllPaginatedAsync(int skip, int limit)
        {
            var entities = await repository.GetAllPaginated(skip, limit);
            var mappedEntities = entities.Adapt<List<TResponse>>();

            var paginatedResponse = new PaginatedResponse<TResponse>
            {
                Entities = mappedEntities,
                TotalCount = mappedEntities.Count,
                Limit = limit,
                Skip = skip,
            };

            return paginatedResponse;
        }

        public virtual async Task<TResponse?> FindByIdAsync(Guid id)
        {
            var entity = await repository.GetByIdAsync(id);
            var mappedEntity = entity.Adapt<TResponse>();
            return mappedEntity;
        }

        public virtual async Task<TResponse> Insert(TRequest entityDto)
        {
            var entity = entityDto.Adapt<TEntity>();
            await repository.Post(entity);
            await repository.SaveChangesAsync();

            return entity.Adapt<TResponse>();
        }

        public virtual async Task<TResponse> Update(TResponse entityDto)
        {
            var entity = entityDto.Adapt<TEntity>();
            repository.Update(entity);
            await repository.SaveChangesAsync();
            return entityDto;
        }

        public virtual async Task DeleteAsync(TResponse entityDto)
        {
            var entity = entityDto.Adapt<TEntity>();
            repository.Delete(entity);
            await repository.SaveChangesAsync();
        }
    }
}
