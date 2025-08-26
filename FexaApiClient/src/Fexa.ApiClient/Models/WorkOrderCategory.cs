using System.Text.Json.Serialization;

namespace Fexa.ApiClient.Models;

public class WorkOrderCategory
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    
    [JsonPropertyName("category")]
    public string Category { get; set; } = string.Empty;
    
    [JsonPropertyName("description")]
    public string? Description { get; set; }
    
    [JsonPropertyName("active")]
    public bool Active { get; set; }
    
    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }
    
    [JsonPropertyName("updated_at")]
    public DateTime UpdatedAt { get; set; }
    
    [JsonPropertyName("created_by")]
    public int CreatedBy { get; set; }
    
    [JsonPropertyName("updated_by")]
    public int UpdatedBy { get; set; }
    
    [JsonPropertyName("parent_id")]
    public int? ParentId { get; set; }
    
    [JsonPropertyName("custom_field_values")]
    public Dictionary<string, object>? CustomFieldValues { get; set; }
    
    [JsonPropertyName("import_id")]
    public int? ImportId { get; set; }
    
    [JsonPropertyName("import_date")]
    public DateTime? ImportDate { get; set; }
    
    [JsonPropertyName("ancestor_ids")]
    public List<int>? AncestorIds { get; set; }
    
    [JsonPropertyName("category_with_all_ancestors")]
    public string? CategoryWithAllAncestors { get; set; }
    
    [JsonPropertyName("first_ancestor_id")]
    public int? FirstAncestorId { get; set; }
    
    [JsonPropertyName("category_with_first_ancestor")]
    public string? CategoryWithFirstAncestor { get; set; }
    
    [JsonPropertyName("first_ancestor_display")]
    public string? FirstAncestorDisplay { get; set; }
    
    [JsonPropertyName("avg_invoice_amt")]
    public string? AvgInvoiceAmt { get; set; }
    
    [JsonPropertyName("parent")]
    public WorkOrderCategory? Parent { get; set; }
    
    [JsonPropertyName("is_leaf")]
    public bool IsLeaf { get; set; }
}

public class WorkOrderCategoriesResponse
{
    [JsonPropertyName("categories")]
    public List<WorkOrderCategory> Categories { get; set; } = new();
}