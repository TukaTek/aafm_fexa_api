namespace Fexa.ApiClient.Models;

public class Visit
{
    public int Id { get; set; }
    public string VisitNumber { get; set; } = string.Empty;
    public DateTime? ScheduledDate { get; set; }
    public DateTime? ActualDate { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public DateTime? CheckInTime { get; set; }
    public DateTime? CheckOutTime { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int? WorkOrderId { get; set; }
    public int? AssignmentId { get; set; }
    public int? TechnicianId { get; set; }
    public int? ClientId { get; set; }
    public int? LocationId { get; set; }
    public int? FacilityId { get; set; }
    public string? StoreName { get; set; }
    public string? Notes { get; set; }
    public string? WorkPerformed { get; set; }
    public string? Scope { get; set; }
    public decimal? Duration { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    
    // Related entities (populated when included)
    public object? WorkOrder { get; set; }
    public object? Technician { get; set; }
    public object? Client { get; set; }
    public object? Location { get; set; }
}