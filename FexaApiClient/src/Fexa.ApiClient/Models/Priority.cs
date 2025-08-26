using System.Text.Json.Serialization;

namespace Fexa.ApiClient.Models;

public class Priority
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
    
    [JsonPropertyName("description")]
    public string? Description { get; set; }
    
    [JsonPropertyName("active")]
    public bool Active { get; set; }
    
    [JsonPropertyName("role_id")]
    public int? RoleId { get; set; }
    
    [JsonPropertyName("facility_id")]
    public int? FacilityId { get; set; }
    
    [JsonPropertyName("category_id")]
    public int? CategoryId { get; set; }
    
    [JsonPropertyName("workorder_class_id")]
    public int? WorkorderClassId { get; set; }
    
    [JsonPropertyName("hours_to_arrive")]
    public string? HoursToArrive { get; set; }
    
    [JsonPropertyName("hours_to_complete")]
    public string? HoursToComplete { get; set; }
    
    [JsonPropertyName("hours_to_respond")]
    public string? HoursToRespond { get; set; }
    
    [JsonPropertyName("created_by")]
    public int CreatedBy { get; set; }
    
    [JsonPropertyName("updated_by")]
    public int UpdatedBy { get; set; }
    
    [JsonPropertyName("severity_legacy")]
    public string? SeverityLegacy { get; set; }
    
    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }
    
    [JsonPropertyName("updated_at")]
    public DateTime UpdatedAt { get; set; }
    
    [JsonPropertyName("default")]
    public bool? Default { get; set; }
    
    [JsonPropertyName("custom_field_values")]
    public Dictionary<string, object>? CustomFieldValues { get; set; }
    
    [JsonPropertyName("color")]
    public string? Color { get; set; }
    
    [JsonPropertyName("severity_id")]
    public int? SeverityId { get; set; }
    
    [JsonPropertyName("severity_name")]
    public string? SeverityName { get; set; }
    
    [JsonPropertyName("import_id")]
    public string? ImportId { get; set; }
    
    [JsonPropertyName("import_date")]
    public DateTime? ImportDate { get; set; }
    
    [JsonPropertyName("trakref_code")]
    public string? TrakrefCode { get; set; }
}

public class PrioritiesResponse
{
    [JsonPropertyName("priorities")]
    public List<Priority> Priorities { get; set; } = new();
}