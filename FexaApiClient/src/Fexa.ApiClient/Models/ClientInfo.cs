using System.Text.Json.Serialization;

namespace Fexa.ApiClient.Models;

/// <summary>
/// Simplified client information for caching - contains just ID and Name
/// </summary>
public class ClientInfo
{
    public int Id { get; set; }
    
    /// <summary>
    /// Client company name (from default_general_address.company or default_billing_address.company)
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// DBA name if different from company name
    /// </summary>
    public string? Dba { get; set; }
    
    /// <summary>
    /// Whether the client is currently active
    /// </summary>
    public bool Active { get; set; }
    
    /// <summary>
    /// IVR ID for quick lookup
    /// </summary>
    public string? IvrId { get; set; }
}

/// <summary>
/// Response wrapper for cached client lookup
/// </summary>
public class CachedClientResponse
{
    [JsonPropertyName("clients")]
    public List<ClientInfo> Clients { get; set; } = new();
    
    [JsonPropertyName("totalCached")]
    public int TotalCached { get; set; }
    
    [JsonPropertyName("cacheAge")]
    public TimeSpan CacheAge { get; set; }
    
    [JsonPropertyName("lastRefreshed")]
    public DateTime LastRefreshed { get; set; }
}

/// <summary>
/// Cache status for client data
/// </summary>
public class ClientCacheStatus
{
    public DateTime LastRefreshed { get; set; }
    public bool IsRefreshing { get; set; }
    public int ItemCount { get; set; }
    public int ActiveCount { get; set; }
    public TimeSpan CacheAge => DateTime.UtcNow - LastRefreshed;
    public DateTime? LastRefreshAttempt { get; set; }
    public bool LastRefreshSuccessful { get; set; }
}