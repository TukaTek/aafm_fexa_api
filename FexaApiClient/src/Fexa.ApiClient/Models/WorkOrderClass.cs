using System.Text.Json.Serialization;

namespace Fexa.ApiClient.Models;

public class WorkOrderClass
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
    
    [JsonPropertyName("description")]
    public string? Description { get; set; }
    
    [JsonPropertyName("active")]
    public bool Active { get; set; }
    
    [JsonPropertyName("default")]
    public bool? Default { get; set; }
    
    [JsonPropertyName("pm_type")]
    public bool PmType { get; set; }
    
    [JsonPropertyName("created_at")]
    public DateTime? CreatedAt { get; set; }
    
    [JsonPropertyName("updated_at")]
    public DateTime? UpdatedAt { get; set; }
    
    [JsonPropertyName("created_by")]
    public int? CreatedBy { get; set; }
    
    [JsonPropertyName("updated_by")]
    public int? UpdatedBy { get; set; }
    
    [JsonPropertyName("custom_field_values")]
    public Dictionary<string, object>? CustomFieldValues { get; set; }
    
    [JsonPropertyName("import_id")]
    public int? ImportId { get; set; }
    
    [JsonPropertyName("import_date")]
    public DateTime? ImportDate { get; set; }
}

public class WorkOrderClassesResponse
{
    [JsonPropertyName("workorder_classes")]
    public List<WorkOrderClass>? WorkOrderClasses { get; set; }
}

// Simplified DTO for Web API responses
public class WorkOrderClassDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool Active { get; set; }
    public bool? Default { get; set; }
    public bool PmType { get; set; }
}