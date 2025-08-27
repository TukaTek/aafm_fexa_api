using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Fexa.ApiClient.WebApi.Models;

/// <summary>
/// Simplified work order creation request with intuitive field names
/// </summary>
public class CreateWorkOrderDto
{
    /// <summary>
    /// Client ID for this work order
    /// </summary>
    [Required]
    [JsonPropertyName("client_id")]
    public int ClientId { get; set; }
    
    /// <summary>
    /// Store/Location ID where the work is needed
    /// </summary>
    [Required]
    [JsonPropertyName("location_id")]
    public int LocationId { get; set; }
    
    /// <summary>
    /// Category ID for the type of work
    /// </summary>
    [Required]
    [JsonPropertyName("category_id")]
    public int CategoryId { get; set; }
    
    /// <summary>
    /// Priority ID (e.g., 12 for standard priority)
    /// </summary>
    [Required]
    [JsonPropertyName("priority_id")]
    public int PriorityId { get; set; }
    
    /// <summary>
    /// Description of the issue or work needed
    /// </summary>
    [Required]
    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// User ID creating this work order (optional - uses default if not provided)
    /// </summary>
    [JsonPropertyName("created_by_user_id")]
    public int? CreatedByUserId { get; set; }
    
    /// <summary>
    /// Work order class ID (optional - defaults to 1 for standard)
    /// </summary>
    [JsonPropertyName("work_order_class_id")]
    public int? WorkOrderClassId { get; set; }
    
    /// <summary>
    /// Client purchase order number (optional)
    /// </summary>
    [JsonPropertyName("client_po_number")]
    public string? ClientPoNumber { get; set; }
    
    /// <summary>
    /// Additional notes (optional)
    /// </summary>
    [JsonPropertyName("notes")]
    public string? Notes { get; set; }
    
    /// <summary>
    /// Scheduled date/time (optional, format: yyyy-MM-dd HH:mm:ss)
    /// </summary>
    [JsonPropertyName("scheduled_for")]
    public string? ScheduledFor { get; set; }
    
    /// <summary>
    /// Vendor ID to assign (optional)
    /// </summary>
    [JsonPropertyName("vendor_id")]
    public int? VendorId { get; set; }
    
    /// <summary>
    /// Custom work order number (optional)
    /// </summary>
    [JsonPropertyName("work_order_number")]
    public string? WorkOrderNumber { get; set; }
    
    /// <summary>
    /// Severity ID (optional)
    /// </summary>
    [JsonPropertyName("severity_id")]
    public int? SeverityId { get; set; }
    
    /// <summary>
    /// Emergency flag (optional)
    /// </summary>
    [JsonPropertyName("is_emergency")]
    public bool? IsEmergency { get; set; }
    
    /// <summary>
    /// NTN number (optional)
    /// </summary>
    [JsonPropertyName("ntn")]
    public string? Ntn { get; set; }
    
    /// <summary>
    /// Client-specific notes (optional)
    /// </summary>
    [JsonPropertyName("client_notes")]
    public string? ClientNotes { get; set; }
}

/// <summary>
/// Simplified response after creating a work order
/// </summary>
public class CreateWorkOrderResponseDto
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    
    [JsonPropertyName("work_order_number")]
    public string? WorkOrderNumber { get; set; }
    
    [JsonPropertyName("status")]
    public string Status { get; set; } = "New";
    
    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;
    
    [JsonPropertyName("client_id")]
    public int ClientId { get; set; }
    
    [JsonPropertyName("location_id")]
    public int LocationId { get; set; }
    
    [JsonPropertyName("category_id")]
    public int CategoryId { get; set; }
    
    [JsonPropertyName("priority_id")]
    public int PriorityId { get; set; }
    
    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }
    
    [JsonPropertyName("client_po_number")]
    public string? ClientPoNumber { get; set; }
    
    [JsonPropertyName("vendor_id")]
    public int? VendorId { get; set; }
}