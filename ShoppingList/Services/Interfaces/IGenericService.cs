using LabWeb.DTOs;
using LabWeb.DTOs.Interfaces;
using LabWeb.Migrations;
using System.Linq.Expressions;

namespace LabWeb.Services.Interfaces
{
    public interface IGenericService<TRequest, TResponse>
        where TRequest : IRequest
        where TResponse : IResponse
    {
        Task DeleteAsync(TResponse entityDto);
        Task<TResponse?> FindByIdAsync(Guid id);
        Task<List<TResponse>> GetAllAsync();
        Task<PaginatedResponse<TResponse>> GetAllPaginatedAsync(int skip, int limit);
        Task<TResponse> Insert(TRequest entityDto);
        Task<TResponse> Update(TResponse entityDto);
    }
}