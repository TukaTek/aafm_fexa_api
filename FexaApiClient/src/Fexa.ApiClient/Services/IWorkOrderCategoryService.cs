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
}