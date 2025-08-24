using System.Text.Json.Serialization;

namespace Fexa.ApiClient.Models;

// Vendor is an alias for Subcontractor in the API
public class Vendor : Subcontractor
{
}

// Response wrapper for the API
public class VendorsResponse
{
    [JsonPropertyName("subcontractors")]
    public List<Vendor> Vendors { get; set; } = new();
    
    [JsonPropertyName("total")]
    public int Total { get; set; }
    
    [JsonPropertyName("start")]
    public int Start { get; set; }
    
    [JsonPropertyName("limit")]
    public int Limit { get; set; }
}

public class VendorResponse
{
    [JsonPropertyName("subcontractors")]
    public List<Vendor>? Vendors { get; set; }
}

// Custom fields specific to vendors
public class VendorCustomFields
{
    [JsonPropertyName("ach")]
    public bool? Ach { get; set; }
    
    [JsonPropertyName("website")]
    public string? Website { get; set; }
    
    [JsonPropertyName("dnu_reason")]
    public string? DnuReason { get; set; }
    
    [JsonPropertyName("qb_account")]
    public string? QbAccount { get; set; }
    
    [JsonPropertyName("date_created")]
    public DateTime? DateCreated { get; set; }
    
    [JsonPropertyName("provider_type")]
    public string? ProviderType { get; set; }
    
    [JsonPropertyName("expiration_date")]
    public DateTime? ExpirationDate { get; set; }
    
    [JsonPropertyName("have_current_w9_")]
    public string? HaveCurrentW9 { get; set; }
}