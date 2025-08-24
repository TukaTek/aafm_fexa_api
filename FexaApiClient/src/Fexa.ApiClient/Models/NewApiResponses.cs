using System.Text.Json.Serialization;

namespace Fexa.ApiClient.Models;

// Response models for new endpoints

public class NotesResponse
{
    [JsonPropertyName("notes")]
    public List<Note> Notes { get; set; } = new List<Note>();
    
    [JsonPropertyName("pagination")]
    public PaginationInfo? Pagination { get; set; }
}

public class SingleNoteResponse
{
    [JsonPropertyName("notes")]
    public Note? Note { get; set; }
}

public class RegionsResponse
{
    [JsonPropertyName("regions")]
    public List<Region> Regions { get; set; } = new List<Region>();
    
    [JsonPropertyName("pagination")]
    public PaginationInfo? Pagination { get; set; }
}

public class SingleRegionResponse
{
    [JsonPropertyName("regions")]
    public Region? Region { get; set; }
}

public class SeveritiesResponse
{
    [JsonPropertyName("severities")]
    public List<Severity> Severities { get; set; } = new List<Severity>();
    
    [JsonPropertyName("pagination")]
    public PaginationInfo? Pagination { get; set; }
}

public class SingleSeverityResponse
{
    [JsonPropertyName("severities")]
    public Severity? Severity { get; set; }
}

public class SubcontractorsResponse
{
    [JsonPropertyName("subcontractors")]
    public List<Subcontractor> Subcontractors { get; set; } = new List<Subcontractor>();
    
    [JsonPropertyName("pagination")]
    public PaginationInfo? Pagination { get; set; }
}

public class SingleSubcontractorResponse
{
    [JsonPropertyName("subcontractors")]
    public Subcontractor? Subcontractor { get; set; }
}
