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
    private const string CACHE_STATUS_KEY = "workorder_categories_status";
    private readonly TimeSpan _cacheExpiration = TimeSpan.FromHours(1); // Cache for 1 hour since these don't change often
    
    // Background refresh state
    private bool _isRefreshing = false;
    private DateTime _lastRefreshAttempt = DateTime.MinValue;
    private bool _lastRefreshSuccessful = true;

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
            _logger.LogInformation("Fetching ALL work order categories from API (with pagination)...");
            
            var allCategories = new List<WorkOrderCategory>();
            var pageSize = 100;
            var currentPage = 0;
            var hasMoreData = true;
            
            while (hasMoreData)
            {
                var start = currentPage * pageSize;
                var endpoint = $"/api/ev1/workorder_categories?start={start}&limit={pageSize}";
                
                _logger.LogDebug("Fetching page {Page} (start={Start}, limit={Limit})", currentPage + 1, start, pageSize);
                
                var response = await _apiService.GetAsync<WorkOrderCategoriesResponse>(endpoint, cancellationToken);
                
                if (response?.Categories != null && response.Categories.Any())
                {
                    allCategories.AddRange(response.Categories);
                    _logger.LogDebug("Fetched page {Page} with {Count} categories. Total so far: {Total}", 
                        currentPage + 1, response.Categories.Count, allCategories.Count);
                    
                    // Check if there are more pages
                    hasMoreData = response.Categories.Count == pageSize;
                }
                else
                {
                    hasMoreData = false;
                }
                
                currentPage++;
                
                // Safety check to prevent infinite loops (max 50 pages = 5000 categories)
                if (currentPage > 50)
                {
                    _logger.LogWarning("Reached maximum page limit, stopping pagination");
                    break;
                }
            }
            
            _logger.LogInformation("Successfully fetched {Count} work order categories across {Pages} pages", 
                allCategories.Count, currentPage);
            
            // Cache the results
            _cache.Set(CACHE_KEY, allCategories, _cacheExpiration);
            
            return allCategories;
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

    // ========== Simplified DTO Methods ==========

    public async Task<CategoryHierarchyResponse> GetSimplifiedCategoriesAsync(CancellationToken cancellationToken = default)
    {
        var allCategories = await GetAllCategoriesAsync(cancellationToken);
        var warnings = new List<string>();
        
        // Build parent ID set for IsLeaf calculation
        var parentIds = new HashSet<int>(allCategories
            .Where(c => c.ParentId.HasValue)
            .Select(c => c.ParentId!.Value));
        
        // Transform and validate
        var simplifiedCategories = new List<CategoryDto>();
        foreach (var category in allCategories)
        {
            var dto = TransformToSimplifiedDto(category, parentIds);
            simplifiedCategories.Add(dto);
            
            // Validate IsLeaf matches Fexa's value
            if (dto.IsLeaf != category.IsLeaf)
            {
                warnings.Add($"IsLeaf mismatch for category {category.Id} '{category.Category}': " +
                           $"Calculated={dto.IsLeaf}, Fexa={category.IsLeaf}");
            }
        }
        
        // Validate hierarchy integrity
        ValidateHierarchy(allCategories, warnings);
        
        // Check for duplicate names at same level
        CheckDuplicateNames(simplifiedCategories, warnings);
        
        if (warnings.Any())
        {
            _logger.LogWarning("Category hierarchy validation warnings: {Warnings}", string.Join("; ", warnings));
        }
        
        return new CategoryHierarchyResponse
        {
            Categories = simplifiedCategories,
            RetrievedAt = DateTime.UtcNow,
            Warnings = warnings.Any() ? warnings : null
        };
    }

    public async Task<List<CategoryDto>> GetActiveSimplifiedCategoriesAsync(CancellationToken cancellationToken = default)
    {
        var response = await GetSimplifiedCategoriesAsync(cancellationToken);
        return response.Categories.Where(c => c.Active).ToList();
    }

    public async Task<List<CategoryDto>> GetChildrenAsync(int parentId, CancellationToken cancellationToken = default)
    {
        var response = await GetSimplifiedCategoriesAsync(cancellationToken);
        return response.Categories.Where(c => c.ParentId == parentId).ToList();
    }

    public async Task<List<CategoryDto>> GetRootCategoriesAsync(CancellationToken cancellationToken = default)
    {
        var response = await GetSimplifiedCategoriesAsync(cancellationToken);
        return response.Categories.Where(c => c.ParentId == null).ToList();
    }

    public async Task<CategoryDto?> GetByFullPathAsync(string fullPath, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(fullPath))
            return null;
            
        var response = await GetSimplifiedCategoriesAsync(cancellationToken);
        return response.Categories.FirstOrDefault(c => 
            string.Equals(c.FullPath, fullPath, StringComparison.OrdinalIgnoreCase));
    }

    // ========== Cache Management Methods ==========

    public async Task RefreshCacheInBackgroundAsync()
    {
        if (_isRefreshing)
        {
            _logger.LogWarning("Cache refresh already in progress, skipping");
            return;
        }

        _ = Task.Run(async () =>
        {
            try
            {
                _isRefreshing = true;
                _lastRefreshAttempt = DateTime.UtcNow;
                _logger.LogInformation("Starting background cache refresh with pagination...");

                // Fetch fresh data with pagination
                var allCategories = new List<WorkOrderCategory>();
                var pageSize = 100;
                var currentPage = 0;
                var hasMoreData = true;
                
                while (hasMoreData)
                {
                    var start = currentPage * pageSize;
                    var endpoint = $"/api/ev1/workorder_categories?start={start}&limit={pageSize}";
                    
                    var response = await _apiService.GetAsync<WorkOrderCategoriesResponse>(endpoint);
                    
                    if (response?.Categories != null && response.Categories.Any())
                    {
                        allCategories.AddRange(response.Categories);
                        _logger.LogDebug("Background refresh: page {Page} with {Count} categories. Total: {Total}", 
                            currentPage + 1, response.Categories.Count, allCategories.Count);
                        
                        hasMoreData = response.Categories.Count == pageSize;
                    }
                    else
                    {
                        hasMoreData = false;
                    }
                    
                    currentPage++;
                    
                    if (currentPage > 50) // Safety limit
                    {
                        _logger.LogWarning("Background refresh: reached maximum page limit");
                        break;
                    }
                }
                
                if (allCategories.Any())
                {
                    // Update cache atomically
                    _cache.Set(CACHE_KEY, allCategories, _cacheExpiration);
                    _lastRefreshSuccessful = true;
                    
                    // Update status
                    var status = new CacheStatusDto
                    {
                        LastRefreshed = DateTime.UtcNow,
                        ItemCount = allCategories.Count,
                        LastRefreshAttempt = _lastRefreshAttempt,
                        LastRefreshSuccessful = true,
                        IsRefreshing = false
                    };
                    _cache.Set(CACHE_STATUS_KEY, status, _cacheExpiration);
                    
                    _logger.LogInformation("Background cache refresh completed successfully with {Count} items across {Pages} pages", 
                        allCategories.Count, currentPage);
                }
                else
                {
                    _lastRefreshSuccessful = false;
                    _logger.LogWarning("Background cache refresh returned no data");
                }
            }
            catch (Exception ex)
            {
                _lastRefreshSuccessful = false;
                _logger.LogError(ex, "Background cache refresh failed");
            }
            finally
            {
                _isRefreshing = false;
            }
        });
    }

    public async Task<CacheStatusDto> GetCacheStatusAsync()
    {
        // Try to get cached status first
        if (_cache.TryGetValue(CACHE_STATUS_KEY, out CacheStatusDto? cachedStatus) && cachedStatus != null)
        {
            cachedStatus.IsRefreshing = _isRefreshing;
            return cachedStatus;
        }
        
        // Build status from current state
        var categories = await GetAllCategoriesAsync();
        return new CacheStatusDto
        {
            LastRefreshed = DateTime.UtcNow.AddMinutes(-30), // Estimate if not cached
            IsRefreshing = _isRefreshing,
            ItemCount = categories.Count,
            LastRefreshAttempt = _lastRefreshAttempt,
            LastRefreshSuccessful = _lastRefreshSuccessful
        };
    }

    public async Task<List<WorkOrderCategory>> RefreshCacheAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Forcing cache refresh");
        _cache.Remove(CACHE_KEY);
        _cache.Remove(CACHE_STATUS_KEY);
        return await GetAllCategoriesAsync(cancellationToken);
    }

    // ========== Private Helper Methods ==========

    private CategoryDto TransformToSimplifiedDto(WorkOrderCategory category, HashSet<int> parentIds)
    {
        return new CategoryDto
        {
            Id = category.Id,
            Category = category.Category ?? string.Empty,
            Description = category.Description,
            ParentId = category.ParentId,
            Active = category.Active,
            IsLeaf = !parentIds.Contains(category.Id),
            FullPath = category.CategoryWithAllAncestors ?? category.Category ?? string.Empty,
            CreatedAt = category.CreatedAt,
            UpdatedAt = category.UpdatedAt
        };
    }

    private void ValidateHierarchy(List<WorkOrderCategory> categories, List<string> warnings)
    {
        var categoryDict = categories.ToDictionary(c => c.Id);
        
        foreach (var category in categories)
        {
            // Check for circular references
            if (HasCircularReference(category, categoryDict))
            {
                warnings.Add($"Circular reference detected for category {category.Id} '{category.Category}'");
            }
            
            // Validate parent exists
            if (category.ParentId.HasValue && !categoryDict.ContainsKey(category.ParentId.Value))
            {
                warnings.Add($"Category {category.Id} '{category.Category}' references non-existent parent {category.ParentId}");
            }
            
            // Cross-check computed path vs provided path
            if (!string.IsNullOrEmpty(category.CategoryWithAllAncestors))
            {
                var computedPath = BuildCategoryPath(category, categoryDict);
                if (!string.Equals(computedPath, category.CategoryWithAllAncestors, StringComparison.OrdinalIgnoreCase))
                {
                    warnings.Add($"Path mismatch for {category.Id}: Computed='{computedPath}' vs Fexa='{category.CategoryWithAllAncestors}'");
                }
            }
        }
    }

    private bool HasCircularReference(WorkOrderCategory category, Dictionary<int, WorkOrderCategory> categoryDict)
    {
        var visited = new HashSet<int>();
        var current = category;
        
        while (current?.ParentId != null)
        {
            if (!visited.Add(current.Id))
                return true; // Circular reference detected
                
            if (!categoryDict.TryGetValue(current.ParentId.Value, out current))
                break;
        }
        
        return false;
    }

    private string BuildCategoryPath(WorkOrderCategory category, Dictionary<int, WorkOrderCategory> categoryDict)
    {
        var path = new List<string>();
        var current = category;
        
        while (current != null)
        {
            path.Insert(0, current.Category ?? string.Empty);
            if (current.ParentId.HasValue && 
                categoryDict.TryGetValue(current.ParentId.Value, out var parent))
            {
                current = parent;
            }
            else
            {
                break;
            }
        }
        
        return string.Join(" | ", path);
    }

    private void CheckDuplicateNames(List<CategoryDto> categories, List<string> warnings)
    {
        var grouped = categories
            .GroupBy(c => new { c.ParentId, Name = c.Category.ToLowerInvariant() })
            .Where(g => g.Count() > 1);
            
        foreach (var group in grouped)
        {
            var duplicates = string.Join(", ", group.Select(c => c.Id));
            warnings.Add($"Duplicate category name '{group.First().Category}' at parent level {group.Key.ParentId}: IDs [{duplicates}]");
        }
    }
}