using System.Text.Json.Serialization;

namespace Fexa.ApiClient.Models;

/// <summary>
/// Simplified DTO for work order categories with preserved hierarchical context
/// </summary>
public class CategoryDto
{
    public int Id { get; set; }
    
    /// <summary>
    /// The category name (e.g., "Grease Trap")
    /// </summary>
    public string Category { get; set; } = string.Empty;
    
    /// <summary>
    /// Category description
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// Parent category ID if this is a subcategory
    /// </summary>
    public int? ParentId { get; set; }
    
    /// <summary>
    /// Whether this category is currently active
    /// </summary>
    public bool Active { get; set; }
    
    /// <summary>
    /// True if this category has no children
    /// </summary>
    public bool IsLeaf { get; set; }
    
    /// <summary>
    /// Full hierarchical path (e.g., "Plumbing | Grease Trap")
    /// Preserves the category_with_all_ancestors from Fexa API
    /// </summary>
    public string FullPath { get; set; } = string.Empty;
    
    public DateTime CreatedAt { get; set; }
    
    public DateTime UpdatedAt { get; set; }
}

/// <summary>
/// Response wrapper for simplified category hierarchy
/// </summary>
public class CategoryHierarchyResponse
{
    [JsonPropertyName("categories")]
    public List<CategoryDto> Categories { get; set; } = new();
    
    [JsonPropertyName("retrievedAt")]
    public DateTime RetrievedAt { get; set; }
    
    /// <summary>
    /// Optional validation warnings found during transformation
    /// </summary>
    [JsonPropertyName("warnings")]
    public List<string>? Warnings { get; set; }
}

/// <summary>
/// Cache status information for monitoring
/// </summary>
public class CacheStatusDto
{
    public DateTime LastRefreshed { get; set; }
    public bool IsRefreshing { get; set; }
    public int ItemCount { get; set; }
    public TimeSpan CacheAge => DateTime.UtcNow - LastRefreshed;
    public DateTime? LastRefreshAttempt { get; set; }
    public bool LastRefreshSuccessful { get; set; }
}