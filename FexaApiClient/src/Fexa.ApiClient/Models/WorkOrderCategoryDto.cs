using System.Text.Json.Serialization;

namespace Fexa.ApiClient.Models;

public class WorkOrderCategoryDto
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    
    [JsonPropertyName("category")]
    public string Category { get; set; } = string.Empty;
    
    [JsonPropertyName("description")]
    public string? Description { get; set; }
    
    [JsonPropertyName("parent_category")]
    public string? ParentCategory { get; set; }
    
    [JsonPropertyName("full_path")]
    public string? FullPath { get; set; }
}