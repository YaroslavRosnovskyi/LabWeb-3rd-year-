namespace LabWeb.DTOs;

public class PaginatedResponse<TMappedEntity>
   where TMappedEntity : BaseDto
{
    public IEnumerable<TMappedEntity> MappedEntities { get; set; }
    public int TotalCount { get; set; }
    public int Limit { get; set; }
    public int Skip { get; set; }
    public string? NextLink { get; set; }
}