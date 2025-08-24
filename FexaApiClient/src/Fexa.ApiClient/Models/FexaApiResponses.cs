using System.Text.Json.Serialization;

namespace Fexa.ApiClient.Models;

// Actual response structures from Fexa API

public class VisitsResponse
{
    [JsonPropertyName("visits")]
    public List<VisitData> Visits { get; set; } = new List<VisitData>();
    
    [JsonPropertyName("pagination")]
    public PaginationInfo? Pagination { get; set; }
}

public class VisitData
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    
    [JsonPropertyName("assignment_id")]
    public int? AssignmentId { get; set; }
    
    [JsonPropertyName("workorder_id")]
    public int? WorkorderId { get; set; }
    
    [JsonPropertyName("check_in_time")]
    public DateTime? CheckInTime { get; set; }
    
    [JsonPropertyName("check_out_time")]
    public DateTime? CheckOutTime { get; set; }
    
    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }
    
    [JsonPropertyName("updated_at")]
    public DateTime? UpdatedAt { get; set; }
    
    [JsonPropertyName("created_by")]
    public int? CreatedBy { get; set; }
    
    [JsonPropertyName("scope")]
    public string? Scope { get; set; }
    
    [JsonPropertyName("work_performed")]
    public string? WorkPerformed { get; set; }
    
    [JsonPropertyName("summary")]
    public string? Summary { get; set; }
    
    [JsonPropertyName("start_date")]
    public DateTime? StartDate { get; set; }
    
    [JsonPropertyName("end_date")]
    public DateTime? EndDate { get; set; }
    
    [JsonPropertyName("facility_id")]
    public int? FacilityId { get; set; }
    
    [JsonPropertyName("object_state")]
    public ObjectState? ObjectState { get; set; }
    
    [JsonPropertyName("store")]
    public Store? Store { get; set; }
    
    [JsonPropertyName("category")]
    public Category? Category { get; set; }
}

public class ObjectState
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    
    [JsonPropertyName("status_id")]
    public int StatusId { get; set; }
    
    [JsonPropertyName("status")]
    public StatusInfo? Status { get; set; }
}

public class StatusInfo
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
    
    [JsonPropertyName("workflow_type")]
    public WorkflowType? WorkflowType { get; set; }
}

public class WorkflowType
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
}

public class Store
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
    
    [JsonPropertyName("identifier")]
    public string Identifier { get; set; } = string.Empty;
}

public class Category
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    
    [JsonPropertyName("category")]
    public string CategoryName { get; set; } = string.Empty;
    
    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;
}

public class InvoicesResponse  
{
    [JsonPropertyName("invoices")]
    public List<object> Invoices { get; set; } = new List<object>();
    
    [JsonPropertyName("pagination")]
    public PaginationInfo? Pagination { get; set; }
}

public class UsersResponse
{
    [JsonPropertyName("user")]
    public List<object> Users { get; set; } = new List<object>();
    
    [JsonPropertyName("pagination")]
    public PaginationInfo? Pagination { get; set; }
}

public class WorkordersResponse
{
    [JsonPropertyName("workorders")]
    public List<object> Workorders { get; set; } = new List<object>();
    
    [JsonPropertyName("pagination")]
    public PaginationInfo? Pagination { get; set; }
}

public class PaginationInfo
{
    [JsonPropertyName("total")]
    public int Total { get; set; }
    
    [JsonPropertyName("per_page")]
    public int PerPage { get; set; }
    
    [JsonPropertyName("current_page")]
    public int CurrentPage { get; set; }
    
    [JsonPropertyName("total_pages")]
    public int TotalPages { get; set; }
}