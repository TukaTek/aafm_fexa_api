using System.Text.Json.Serialization;

namespace Fexa.ApiClient.Models;

public class Assignment
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    
    [JsonPropertyName("workorder_id")]
    public int? WorkOrderId { get; set; }
    
    [JsonPropertyName("role_id")]
    public int? RoleId { get; set; }
    
    [JsonPropertyName("spoke_with")]
    public string? SpokeWith { get; set; }
    
    [JsonPropertyName("created_at")]
    public DateTime? CreatedAt { get; set; }
    
    [JsonPropertyName("updated_at")]
    public DateTime? UpdatedAt { get; set; }
    
    [JsonPropertyName("created_by")]
    public int? CreatedBy { get; set; }
    
    [JsonPropertyName("updated_by")]
    public int? UpdatedBy { get; set; }
    
    [JsonPropertyName("scope")]
    public string? Scope { get; set; }
    
    [JsonPropertyName("category_id")]
    public int? CategoryId { get; set; }
    
    [JsonPropertyName("facility_id")]
    public int? FacilityId { get; set; }
    
    [JsonPropertyName("custom_field_values")]
    public Dictionary<string, object>? CustomFieldValues { get; set; }
    
    [JsonPropertyName("default_assignment_category_id")]
    public int? DefaultAssignmentCategoryId { get; set; }
    
    [JsonPropertyName("rank")]
    public int? Rank { get; set; }
    
    [JsonPropertyName("initial_response_deadline")]
    public DateTime? InitialResponseDeadline { get; set; }
    
    [JsonPropertyName("initial_arrival_deadline")]
    public DateTime? InitialArrivalDeadline { get; set; }
    
    [JsonPropertyName("completion_deadline")]
    public DateTime? CompletionDeadline { get; set; }
    
    [JsonPropertyName("priority_id")]
    public int? PriorityId { get; set; }
    
    [JsonPropertyName("past_initial_response_deadline")]
    public bool? PastInitialResponseDeadline { get; set; }
    
    [JsonPropertyName("past_initial_arrival_deadline")]
    public bool? PastInitialArrivalDeadline { get; set; }
    
    [JsonPropertyName("past_completion_deadline")]
    public bool? PastCompletionDeadline { get; set; }
    
    [JsonPropertyName("date_completed")]
    public DateTime? DateCompleted { get; set; }
    
    [JsonPropertyName("date_accepted")]
    public DateTime? DateAccepted { get; set; }
    
    [JsonPropertyName("initial_response_date")]
    public DateTime? InitialResponseDate { get; set; }
    
    [JsonPropertyName("object_state")]
    public AssignmentObjectState? ObjectState { get; set; }
    
    [JsonPropertyName("subcontractor_not_to_exceed")]
    public SubcontractorNotToExceed? SubcontractorNotToExceed { get; set; }
}

public class AssignmentObjectState
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    
    [JsonPropertyName("object_id")]
    public int? ObjectId { get; set; }
    
    [JsonPropertyName("status_id")]
    public int? StatusId { get; set; }
    
    [JsonPropertyName("last_status_id")]
    public int? LastStatusId { get; set; }
    
    [JsonPropertyName("status_last_changed")]
    public DateTime? StatusLastChanged { get; set; }
    
    [JsonPropertyName("status")]
    public AssignmentStatus? Status { get; set; }
}

public class AssignmentStatus
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    
    [JsonPropertyName("name")]
    public string? Name { get; set; }
    
    [JsonPropertyName("workflow_type")]
    public WorkflowType? WorkflowType { get; set; }
}

public class SubcontractorNotToExceed
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    
    [JsonPropertyName("amount")]
    public string? Amount { get; set; }
    
    [JsonPropertyName("exchanged_info")]
    public ExchangedInfo? ExchangedInfo { get; set; }
}

public class ExchangedInfo
{
    [JsonPropertyName("amount")]
    public decimal? Amount { get; set; }
    
    [JsonPropertyName("vendor_amount")]
    public decimal? VendorAmount { get; set; }
}