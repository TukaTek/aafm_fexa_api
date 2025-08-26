using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Fexa.ApiClient.Models;

namespace Fexa.ApiClient.Services;

public class WorkOrderCategoryService : IWorkOrderCategoryService
{
    private readonly IFexaApiService _apiService;
    private readonly ILogger<WorkOrderCategoryService> _logger;
    private readonly IMemoryCache _cache;
    private const string CACHE_KEY = "workorder_categories_all";
    private readonly TimeSpan _cacheExpiration = TimeSpan.FromHours(1); // Cache for 1 hour since these don't change often

    public WorkOrderCategoryService(
        IFexaApiService apiService,
        ILogger<WorkOrderCategoryService> logger,
        IMemoryCache cache)
    {
        _apiService = apiService ?? throw new ArgumentNullException(nameof(apiService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
    }

    public async Task<List<WorkOrderCategory>> GetAllCategoriesAsync(CancellationToken cancellationToken = default)
    {
        // Check cache first
        if (_cache.TryGetValue(CACHE_KEY, out List<WorkOrderCategory>? cachedCategories))
        {
            _logger.LogDebug("Returning cached work order categories");
            return cachedCategories ?? new List<WorkOrderCategory>();
        }

        try
        {
            _logger.LogInformation("Fetching all work order categories from API");
            
            // Call the API endpoint
            var response = await _apiService.GetAsync<WorkOrderCategoriesResponse>("/api/ev1/categories", cancellationToken);
            
            if (response?.Categories != null)
            {
                _logger.LogInformation("Successfully fetched {Count} work order categories", response.Categories.Count);
                
                // Cache the results
                _cache.Set(CACHE_KEY, response.Categories, _cacheExpiration);
                
                return response.Categories;
            }
            
            _logger.LogWarning("No work order categories returned from API");
            return new List<WorkOrderCategory>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching work order categories");
            throw;
        }
    }

    public async Task<List<WorkOrderCategory>> GetActiveCategoriesAsync(CancellationToken cancellationToken = default)
    {
        var allCategories = await GetAllCategoriesAsync(cancellationToken);
        return allCategories.Where(c => c.Active).ToList();
    }

    public async Task<WorkOrderCategory?> GetCategoryByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var allCategories = await GetAllCategoriesAsync(cancellationToken);
        return allCategories.FirstOrDefault(c => c.Id == id);
    }

    public async Task<WorkOrderCategory?> GetCategoryByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(name))
            return null;
            
        var allCategories = await GetAllCategoriesAsync(cancellationToken);
        return allCategories.FirstOrDefault(c => 
            string.Equals(c.Category, name, StringComparison.OrdinalIgnoreCase));
    }

    public async Task<List<WorkOrderCategory>> GetLeafCategoriesAsync(CancellationToken cancellationToken = default)
    {
        var allCategories = await GetAllCategoriesAsync(cancellationToken);
        return allCategories.Where(c => c.IsLeaf).ToList();
    }

    public async Task<List<WorkOrderCategory>> GetParentCategoriesAsync(CancellationToken cancellationToken = default)
    {
        var allCategories = await GetAllCategoriesAsync(cancellationToken);
        return allCategories.Where(c => c.ParentId == null).ToList();
    }
}