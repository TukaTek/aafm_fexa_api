using Fexa.ApiClient.Models;

namespace Fexa.ApiClient.Services;

public interface IWorkOrderCategoryService
{
    /// <summary>
    /// Gets all work order categories from the API (no paging needed)
    /// </summary>
    Task<List<WorkOrderCategory>> GetAllCategoriesAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets only active categories
    /// </summary>
    Task<List<WorkOrderCategory>> GetActiveCategoriesAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets a category by ID (from cached list)
    /// </summary>
    Task<WorkOrderCategory?> GetCategoryByIdAsync(int id, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets a category by name (from cached list)
    /// </summary>
    Task<WorkOrderCategory?> GetCategoryByNameAsync(string name, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets only leaf categories (categories with no children)
    /// </summary>
    Task<List<WorkOrderCategory>> GetLeafCategoriesAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets only parent categories (categories with parent_id = null)
    /// </summary>
    Task<List<WorkOrderCategory>> GetParentCategoriesAsync(CancellationToken cancellationToken = default);
    
    // ========== Simplified DTO Methods ==========
    
    /// <summary>
    /// Gets all categories as simplified DTOs with hierarchical context preserved
    /// </summary>
    Task<CategoryHierarchyResponse> GetSimplifiedCategoriesAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets only active categories as simplified DTOs
    /// </summary>
    Task<List<CategoryDto>> GetActiveSimplifiedCategoriesAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets children of a specific category as simplified DTOs
    /// </summary>
    Task<List<CategoryDto>> GetChildrenAsync(int parentId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets root categories (where ParentId == null) as simplified DTOs
    /// </summary>
    Task<List<CategoryDto>> GetRootCategoriesAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets a category by its full hierarchical path (e.g., "Plumbing | Grease Trap")
    /// </summary>
    Task<CategoryDto?> GetByFullPathAsync(string fullPath, CancellationToken cancellationToken = default);
    
    // ========== Cache Management Methods ==========
    
    /// <summary>
    /// Refreshes the cache asynchronously in the background
    /// </summary>
    Task RefreshCacheInBackgroundAsync();
    
    /// <summary>
    /// Gets the current cache status for monitoring
    /// </summary>
    Task<CacheStatusDto> GetCacheStatusAsync();
    
    /// <summary>
    /// Forces a cache refresh and waits for completion
    /// </summary>
    Task<List<WorkOrderCategory>> RefreshCacheAsync(CancellationToken cancellationToken = default);
}