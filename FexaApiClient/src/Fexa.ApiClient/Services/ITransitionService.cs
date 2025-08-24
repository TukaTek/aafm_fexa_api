using Fexa.ApiClient.Models;

namespace Fexa.ApiClient.Services;

public interface ITransitionService
{
    /// <summary>
    /// Gets a single page of workflow transitions from the API
    /// </summary>
    Task<TransitionsResponse> GetTransitionsAsync(int start = 0, int limit = 100, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets ALL workflow transitions from the API by fetching all pages
    /// </summary>
    Task<List<WorkflowTransition>> GetAllTransitionsAsync(int maxPages = 10, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets unique statuses from all transitions
    /// </summary>
    Task<List<WorkflowStatus>> GetUniqueStatusesAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets transitions filtered by workflow object type
    /// </summary>
    Task<List<WorkflowTransition>> GetTransitionsByTypeAsync(string workflowObjectType, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets possible transitions from a specific status
    /// </summary>
    Task<List<WorkflowTransition>> GetTransitionsFromStatusAsync(int statusId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets possible transitions to a specific status
    /// </summary>
    Task<List<WorkflowTransition>> GetTransitionsToStatusAsync(int statusId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets all unique Work Order (Assignment) statuses
    /// </summary>
    Task<List<WorkflowStatus>> GetWorkOrderStatusesAsync(CancellationToken cancellationToken = default);
}