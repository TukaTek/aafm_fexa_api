namespace Fexa.ApiClient.Models;

/// <summary>
/// Query parameters specific to the Visits endpoint.
/// The Visits endpoint uses direct query parameters instead of the filters array.
/// </summary>
public class VisitQueryParameters
{
    // Pagination
    public int Start { get; set; } = 0;
    public int Limit { get; set; } = 20;
    
    // Direct filter parameters (not using filters array)
    public int? WorkorderId { get; set; }
    public int? TechnicianId { get; set; }
    public int? ClientId { get; set; }
    public int? LocationId { get; set; }
    public string? Status { get; set; }
    public DateTime? ScheduledDateFrom { get; set; }
    public DateTime? ScheduledDateTo { get; set; }
    public DateTime? ActualDateFrom { get; set; }
    public DateTime? ActualDateTo { get; set; }
    
    public Dictionary<string, string> ToDictionary()
    {
        var dict = new Dictionary<string, string>
        {
            ["start"] = Start.ToString(),
            ["limit"] = Limit.ToString()
        };
        
        if (WorkorderId.HasValue)
            dict["workorder_id"] = WorkorderId.Value.ToString();
            
        if (TechnicianId.HasValue)
            dict["technician_id"] = TechnicianId.Value.ToString();
            
        if (ClientId.HasValue)
            dict["client_id"] = ClientId.Value.ToString();
            
        if (LocationId.HasValue)
            dict["location_id"] = LocationId.Value.ToString();
            
        if (!string.IsNullOrWhiteSpace(Status))
            dict["status"] = Status;
            
        if (ScheduledDateFrom.HasValue)
            dict["scheduled_date_from"] = ScheduledDateFrom.Value.ToString("yyyy-MM-dd");
            
        if (ScheduledDateTo.HasValue)
            dict["scheduled_date_to"] = ScheduledDateTo.Value.ToString("yyyy-MM-dd");
            
        if (ActualDateFrom.HasValue)
            dict["actual_date_from"] = ActualDateFrom.Value.ToString("yyyy-MM-dd");
            
        if (ActualDateTo.HasValue)
            dict["actual_date_to"] = ActualDateTo.Value.ToString("yyyy-MM-dd");
            
        return dict;
    }
}