using System.Text.Json.Serialization;

namespace Fexa.ApiClient.Models;

public class WorkOrder
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    
    [JsonPropertyName("workorder_number")]
    public string? WorkOrderNumber { get; set; }
    
    [JsonPropertyName("object_state")]
    public ObjectState? ObjectState { get; set; }
    
    [JsonPropertyName("priority_id")]
    public int? PriorityId { get; set; }
    
    [JsonPropertyName("description")]
    public string? Description { get; set; }
    
    [JsonPropertyName("placed_for")]
    public int? ClientId { get; set; }
    
    [JsonPropertyName("client_name")]
    public string? ClientName { get; set; }
    
    [JsonPropertyName("assigned_to")]
    public int? AssignedTo { get; set; }
    
    [JsonPropertyName("vendor_name")]
    public string? VendorName { get; set; }
    
    [JsonPropertyName("store_id")]
    public int? StoreId { get; set; }
    
    [JsonPropertyName("store_name")]
    public string? StoreName { get; set; }
    
    [JsonPropertyName("store_address")]
    public string? StoreAddress { get; set; }
    
    [JsonPropertyName("store_city")]
    public string? StoreCity { get; set; }
    
    [JsonPropertyName("store_state")]
    public string? StoreState { get; set; }
    
    [JsonPropertyName("store_zip")]
    public string? StoreZip { get; set; }
    
    [JsonPropertyName("technician_id")]
    public int? TechnicianId { get; set; }
    
    [JsonPropertyName("technician_name")]
    public string? TechnicianName { get; set; }
    
    [JsonPropertyName("next_visit")]
    public DateTime? NextVisit { get; set; }
    
    [JsonPropertyName("date_completed")]
    public DateTime? DateCompleted { get; set; }
    
    [JsonPropertyName("created_at")]
    public DateTime? CreatedAt { get; set; }
    
    [JsonPropertyName("updated_at")]
    public DateTime? UpdatedAt { get; set; }
    
    [JsonPropertyName("total_amount")]
    public decimal? TotalAmount { get; set; }
    
    [JsonPropertyName("notes")]
    public string? Notes { get; set; }
    
    [JsonPropertyName("assignments")]
    public List<Assignment>? Assignments { get; set; }
    
    // Helper property to get status from ObjectState
    [JsonIgnore]
    public string Status => ObjectState?.Status?.Name ?? "Unknown";
}