using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Fexa.ApiClient.Models;

namespace Fexa.ApiClient.Services;

public class PriorityService : IPriorityService
{
    private readonly IFexaApiService _apiService;
    private readonly ILogger<PriorityService> _logger;
    private readonly IMemoryCache _cache;
    private const string CACHE_KEY = "priorities_all";
    private readonly TimeSpan _cacheExpiration = TimeSpan.FromHours(1); // Cache for 1 hour since these don't change often

    public PriorityService(
        IFexaApiService apiService,
        ILogger<PriorityService> logger,
        IMemoryCache cache)
    {
        _apiService = apiService ?? throw new ArgumentNullException(nameof(apiService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
    }

    public async Task<List<Priority>> GetAllPrioritiesAsync(CancellationToken cancellationToken = default)
    {
        // Check cache first
        if (_cache.TryGetValue(CACHE_KEY, out List<Priority>? cachedPriorities))
        {
            _logger.LogDebug("Returning cached priorities");
            return cachedPriorities ?? new List<Priority>();
        }

        try
        {
            _logger.LogInformation("Fetching all priorities from API");
            
            // Call the API endpoint
            var response = await _apiService.GetAsync<PrioritiesResponse>("/api/ev1/priorities", cancellationToken);
            
            if (response?.Priorities != null)
            {
                _logger.LogInformation("Successfully fetched {Count} priorities", response.Priorities.Count);
                
                // Cache the results
                _cache.Set(CACHE_KEY, response.Priorities, _cacheExpiration);
                
                return response.Priorities;
            }
            
            _logger.LogWarning("No priorities returned from API");
            return new List<Priority>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching priorities");
            throw;
        }
    }

    public async Task<List<Priority>> GetActivePrioritiesAsync(CancellationToken cancellationToken = default)
    {
        var allPriorities = await GetAllPrioritiesAsync(cancellationToken);
        return allPriorities.Where(p => p.Active).ToList();
    }

    public async Task<Priority?> GetPriorityByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var allPriorities = await GetAllPrioritiesAsync(cancellationToken);
        return allPriorities.FirstOrDefault(p => p.Id == id);
    }

    public async Task<Priority?> GetPriorityByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(name))
            return null;
            
        var allPriorities = await GetAllPrioritiesAsync(cancellationToken);
        return allPriorities.FirstOrDefault(p => 
            string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase));
    }
}