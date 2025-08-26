using Fexa.ApiClient.Models;

namespace Fexa.ApiClient.Services;

public interface IPriorityService
{
    /// <summary>
    /// Gets all priorities from the API (no paging needed)
    /// </summary>
    Task<List<Priority>> GetAllPrioritiesAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets only active priorities
    /// </summary>
    Task<List<Priority>> GetActivePrioritiesAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets a priority by ID (from cached list)
    /// </summary>
    Task<Priority?> GetPriorityByIdAsync(int id, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets a priority by name (from cached list)
    /// </summary>
    Task<Priority?> GetPriorityByNameAsync(string name, CancellationToken cancellationToken = default);
}