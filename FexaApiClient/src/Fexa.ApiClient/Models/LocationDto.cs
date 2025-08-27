namespace Fexa.ApiClient.Models;

public class LocationDto
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? Identifier { get; set; }
    public string? FacilityCode { get; set; }
    public bool Active { get; set; }
    public int? OccupiedBy { get; set; }
    public string? ClientCompany { get; set; }
    public string? LocationType { get; set; }
    public object? SqFootage { get; set; }  // Can be int, decimal, string, or null
    
    // Address information
    public string? Address1 { get; set; }
    public string? Address2 { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? PostalCode { get; set; }
    public string? Country { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    
    // Geographic information
    public string? Latitude { get; set; }
    public string? Longitude { get; set; }
    public string? Timezone { get; set; }
    
    // Dates
    public DateTime? OpenDate { get; set; }
    public DateTime? CloseDate { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}