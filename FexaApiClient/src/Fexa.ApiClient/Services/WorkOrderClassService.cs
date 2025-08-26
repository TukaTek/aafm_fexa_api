using Fexa.ApiClient.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Fexa.ApiClient.Services;

public class WorkOrderClassService : IWorkOrderClassService
{
    private readonly IFexaApiService _apiService;
    private readonly IMemoryCache _cache;
    private readonly ILogger<WorkOrderClassService> _logger;
    private const string CacheKey = "workorder_classes";
    private readonly MemoryCacheEntryOptions _cacheOptions;

    public WorkOrderClassService(
        IFexaApiService apiService,
        IMemoryCache cache,
        ILogger<WorkOrderClassService> logger)
    {
        _apiService = apiService;
        _cache = cache;
        _logger = logger;
        
        // Cache for 1 hour
        _cacheOptions = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(TimeSpan.FromHours(1))
            .SetSlidingExpiration(TimeSpan.FromMinutes(30));
    }

    public async Task<List<WorkOrderClass>> GetAllWorkOrderClassesAsync(CancellationToken cancellationToken = default)
    {
        // Try to get from cache
        if (_cache.TryGetValue<List<WorkOrderClass>>(CacheKey, out var cachedClasses) && cachedClasses != null)
        {
            _logger.LogDebug("Returning {Count} work order classes from cache", cachedClasses.Count);
            return cachedClasses;
        }

        // Fetch from API
        _logger.LogInformation("Fetching work order classes from Fexa API");
        var response = await _apiService.GetAsync<WorkOrderClassesResponse>("/api/ev1/workorder_classes", cancellationToken);
        
        var classes = response?.WorkOrderClasses ?? new List<WorkOrderClass>();
        
        // Store in cache
        _cache.Set(CacheKey, classes, _cacheOptions);
        _logger.LogInformation("Cached {Count} work order classes", classes.Count);
        
        return classes;
    }

    public async Task<List<WorkOrderClass>> GetActiveWorkOrderClassesAsync(CancellationToken cancellationToken = default)
    {
        var allClasses = await GetAllWorkOrderClassesAsync(cancellationToken);
        var activeClasses = allClasses.Where(c => c.Active).ToList();
        
        _logger.LogDebug("Filtered {Active} active work order classes from {Total} total", 
            activeClasses.Count, allClasses.Count);
        
        return activeClasses;
    }

    public async Task<WorkOrderClass?> GetWorkOrderClassByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var allClasses = await GetAllWorkOrderClassesAsync(cancellationToken);
        return allClasses.FirstOrDefault(c => c.Id == id);
    }

    public async Task<WorkOrderClass?> GetWorkOrderClassByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(name))
            return null;
            
        var allClasses = await GetAllWorkOrderClassesAsync(cancellationToken);
        return allClasses.FirstOrDefault(c => 
            string.Equals(c.Name, name, StringComparison.OrdinalIgnoreCase));
    }

    public async Task<List<WorkOrderClass>> RefreshWorkOrderClassesAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Forcing refresh of work order classes cache");
        _cache.Remove(CacheKey);
        return await GetAllWorkOrderClassesAsync(cancellationToken);
    }
}