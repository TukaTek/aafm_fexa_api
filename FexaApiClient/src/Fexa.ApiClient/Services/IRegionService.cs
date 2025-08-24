using Fexa.ApiClient.Models;

namespace Fexa.ApiClient.Services;

public interface IRegionService
{
    Task<PagedResponse<Region>> GetRegionsAsync(QueryParameters? parameters = null, CancellationToken cancellationToken = default);
    Task<Region> GetRegionAsync(int regionId, CancellationToken cancellationToken = default);
    Task<PagedResponse<Region>> GetRegionsByParentAsync(int parentId, QueryParameters? parameters = null, CancellationToken cancellationToken = default);
    Task<PagedResponse<Region>> GetActiveRegionsAsync(QueryParameters? parameters = null, CancellationToken cancellationToken = default);
    Task<PagedResponse<Region>> GetRegionsByLevelAsync(int level, QueryParameters? parameters = null, CancellationToken cancellationToken = default);
    
    // Pagination helpers - fetch all pages
    Task<List<Region>> GetAllRegionsAsync(QueryParameters? baseParameters = null, int maxPages = 10, CancellationToken cancellationToken = default);
    Task<List<Region>> GetAllActiveRegionsAsync(QueryParameters? baseParameters = null, int maxPages = 10, CancellationToken cancellationToken = default);
    
    // CRUD operations
    Task<Region> CreateRegionAsync(CreateRegionRequest request, CancellationToken cancellationToken = default);
    Task<Region> UpdateRegionAsync(int regionId, UpdateRegionRequest request, CancellationToken cancellationToken = default);
    Task DeleteRegionAsync(int regionId, CancellationToken cancellationToken = default);
}

public class CreateRegionRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Code { get; set; }
    public bool? Active { get; set; }
    public int? ParentId { get; set; }
    public int? Level { get; set; }
    public string? Timezone { get; set; }
    public Dictionary<string, object>? CustomFieldValues { get; set; }
}

public class UpdateRegionRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? Code { get; set; }
    public bool? Active { get; set; }
    public int? ParentId { get; set; }
    public int? Level { get; set; }
    public string? Timezone { get; set; }
    public Dictionary<string, object>? CustomFieldValues { get; set; }
}
