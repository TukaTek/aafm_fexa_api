using Fexa.ApiClient.Models;

namespace Fexa.ApiClient.Services;

/// <summary>
/// Service for cached client lookups - provides fast access to client ID and Name
/// </summary>
public interface ICachedClientService
{
    /// <summary>
    /// Gets all cached client info (ID and Name only)
    /// </summary>
    Task<List<ClientInfo>> GetAllClientInfoAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets only active clients from cache
    /// </summary>
    Task<List<ClientInfo>> GetActiveClientInfoAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets a specific client by ID from cache
    /// </summary>
    Task<ClientInfo?> GetClientInfoByIdAsync(int id, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets a client by name (searches company and DBA)
    /// </summary>
    Task<ClientInfo?> GetClientInfoByNameAsync(string name, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets a client by IVR ID
    /// </summary>
    Task<ClientInfo?> GetClientInfoByIvrIdAsync(string ivrId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Search clients by partial name match
    /// </summary>
    Task<List<ClientInfo>> SearchClientInfoAsync(string searchTerm, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Forces a cache refresh and waits for completion
    /// </summary>
    Task<List<ClientInfo>> RefreshCacheAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Refreshes the cache asynchronously in the background
    /// </summary>
    Task RefreshCacheInBackgroundAsync();
    
    /// <summary>
    /// Gets the current cache status for monitoring
    /// </summary>
    Task<ClientCacheStatus> GetCacheStatusAsync();
}