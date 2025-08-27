using System.Text.Json.Serialization;

namespace Fexa.ApiClient.Models;

public class CreateWorkOrderRequest
{
    [JsonPropertyName("workorders")]
    public WorkOrderData Workorders { get; set; } = new();
}

public class WorkOrderData
{
    [JsonPropertyName("workorder_class_id")]
    public int WorkorderClassId { get; set; }
    
    [JsonPropertyName("priority_id")]
    public int PriorityId { get; set; }
    
    [JsonPropertyName("category_id")]
    public int CategoryId { get; set; }
    
    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;
    
    [JsonPropertyName("facility_id")]
    public int FacilityId { get; set; }
    
    [JsonPropertyName("placed_by")]
    public int PlacedBy { get; set; }
    
    [JsonPropertyName("placed_for")]
    public int PlacedFor { get; set; }
    
    [JsonPropertyName("client_purchase_order_numbers")]
    public List<ClientPurchaseOrder> ClientPurchaseOrderNumbers { get; set; } = new();
    
    [JsonPropertyName("notes")]
    public string? Notes { get; set; }
    
    [JsonPropertyName("scheduled_for")]
    public string? ScheduledFor { get; set; }
    
    [JsonPropertyName("vendor_id")]
    public int? VendorId { get; set; }
    
    [JsonPropertyName("work_order_number")]
    public string? WorkOrderNumber { get; set; }
    
    [JsonPropertyName("severity_id")]
    public int? SeverityId { get; set; }
    
    [JsonPropertyName("emergency")]
    public bool? Emergency { get; set; }
    
    [JsonPropertyName("ntn")]
    public string? Ntn { get; set; }
    
    [JsonPropertyName("client_notes")]
    public string? ClientNotes { get; set; }
}

public class ClientPurchaseOrder
{
    [JsonPropertyName("purchase_order_number")]
    public string PurchaseOrderNumber { get; set; } = string.Empty;
    
    [JsonPropertyName("active")]
    public bool Active { get; set; } = true;
}

public class CreateWorkOrderResponse
{
    [JsonPropertyName("workorders")]
    public WorkOrder? Workorders { get; set; }
    
    [JsonPropertyName("success")]
    public bool Success { get; set; }
    
    [JsonPropertyName("message")]
    public string? Message { get; set; }
    
    [JsonPropertyName("errors")]
    public Dictionary<string, List<string>>? Errors { get; set; }
}