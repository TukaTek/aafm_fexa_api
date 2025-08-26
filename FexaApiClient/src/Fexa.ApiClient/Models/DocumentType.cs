using System.Text.Json.Serialization;

namespace Fexa.ApiClient.Models;

public class DocumentType
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
    
    [JsonPropertyName("active")]
    public bool Active { get; set; }
    
    [JsonPropertyName("description")]
    public string? Description { get; set; }
    
    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }
    
    [JsonPropertyName("updated_at")]
    public DateTime UpdatedAt { get; set; }
    
    [JsonPropertyName("import_id")]
    public string? ImportId { get; set; }
    
    [JsonPropertyName("import_date")]
    public DateTime? ImportDate { get; set; }
    
    [JsonPropertyName("custom_field_values")]
    public Dictionary<string, object>? CustomFieldValues { get; set; }
}

public class DocumentTypesResponse
{
    [JsonPropertyName("document_types")]
    public List<DocumentType> DocumentTypes { get; set; } = new();
}