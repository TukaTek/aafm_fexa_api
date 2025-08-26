using Fexa.ApiClient.Models;

namespace Fexa.ApiClient.Services;

public interface IWorkOrderClassService
{
    /// <summary>
    /// Get all work order classes from Fexa API (with caching)
    /// </summary>
    Task<List<WorkOrderClass>> GetAllWorkOrderClassesAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get only active work order classes (filtered from cached data)
    /// </summary>
    Task<List<WorkOrderClass>> GetActiveWorkOrderClassesAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get work order class by ID (from cached data)
    /// </summary>
    Task<WorkOrderClass?> GetWorkOrderClassByIdAsync(int id, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get work order class by name (from cached data)
    /// </summary>
    Task<WorkOrderClass?> GetWorkOrderClassByNameAsync(string name, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Clear the cache and force refresh from API
    /// </summary>
    Task<List<WorkOrderClass>> RefreshWorkOrderClassesAsync(CancellationToken cancellationToken = default);
}