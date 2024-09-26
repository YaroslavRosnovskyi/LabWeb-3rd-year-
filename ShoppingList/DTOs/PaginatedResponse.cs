using LabWeb.DTOs.Interfaces;

namespace LabWeb.DTOs;

public class PaginatedResponse<TResponse>
   where TResponse : IResponse
{
    public IEnumerable<TResponse> Entities { get; set; }
    public int TotalCount { get; set; }
    public int Limit { get; set; }
    public int Skip { get; set; }
    public string? NextLink { get; set; }
}