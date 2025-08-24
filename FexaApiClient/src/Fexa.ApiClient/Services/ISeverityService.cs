using Fexa.ApiClient.Models;

namespace Fexa.ApiClient.Services;

public interface ISeverityService
{
    Task<PagedResponse<Severity>> GetSeveritiesAsync(QueryParameters? parameters = null, CancellationToken cancellationToken = default);
    Task<Severity> GetSeverityAsync(int severityId, CancellationToken cancellationToken = default);
    Task<PagedResponse<Severity>> GetActiveSeveritiesAsync(QueryParameters? parameters = null, CancellationToken cancellationToken = default);
    Task<PagedResponse<Severity>> GetSeveritiesByLevelAsync(int level, QueryParameters? parameters = null, CancellationToken cancellationToken = default);
    
    // Pagination helpers - fetch all pages
    Task<List<Severity>> GetAllSeveritiesAsync(QueryParameters? baseParameters = null, int maxPages = 10, CancellationToken cancellationToken = default);
    Task<List<Severity>> GetAllActiveSeveritiesAsync(QueryParameters? baseParameters = null, int maxPages = 10, CancellationToken cancellationToken = default);
    
    // CRUD operations
    Task<Severity> CreateSeverityAsync(CreateSeverityRequest request, CancellationToken cancellationToken = default);
    Task<Severity> UpdateSeverityAsync(int severityId, UpdateSeverityRequest request, CancellationToken cancellationToken = default);
    Task DeleteSeverityAsync(int severityId, CancellationToken cancellationToken = default);
}

public class CreateSeverityRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? Level { get; set; }
    public string? Color { get; set; }
    public bool? Active { get; set; }
    public int? ResponseTimeHours { get; set; }
    public int? ResolutionTimeHours { get; set; }
    public Dictionary<string, object>? CustomFieldValues { get; set; }
}

public class UpdateSeverityRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public int? Level { get; set; }
    public string? Color { get; set; }
    public bool? Active { get; set; }
    public int? ResponseTimeHours { get; set; }
    public int? ResolutionTimeHours { get; set; }
    public Dictionary<string, object>? CustomFieldValues { get; set; }
}
