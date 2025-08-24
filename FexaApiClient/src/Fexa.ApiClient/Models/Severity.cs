using System.Text.Json.Serialization;

namespace Fexa.ApiClient.Models;

public class Severity
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
    
    [JsonPropertyName("description")]
    public string? Description { get; set; }
    
    [JsonPropertyName("level")]
    public int? Level { get; set; }
    
    [JsonPropertyName("color")]
    public string? Color { get; set; }
    
    [JsonPropertyName("active")]
    public bool? Active { get; set; }
    
    [JsonPropertyName("created_at")]
    public DateTime? CreatedAt { get; set; }
    
    [JsonPropertyName("updated_at")]
    public DateTime? UpdatedAt { get; set; }
    
    [JsonPropertyName("created_by")]
    public int? CreatedBy { get; set; }
    
    [JsonPropertyName("updated_by")]
    public int? UpdatedBy { get; set; }
    
    [JsonPropertyName("response_time_hours")]
    public int? ResponseTimeHours { get; set; }
    
    [JsonPropertyName("resolution_time_hours")]
    public int? ResolutionTimeHours { get; set; }
    
    [JsonPropertyName("custom_field_values")]
    public Dictionary<string, object>? CustomFieldValues { get; set; }
}
