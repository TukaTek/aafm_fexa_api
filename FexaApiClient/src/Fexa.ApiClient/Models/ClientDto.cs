using System.Text.Json.Serialization;

namespace Fexa.ApiClient.Models;

public class ClientDto
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    
    [JsonPropertyName("company")]
    public string? Company { get; set; }
    
    [JsonPropertyName("dba")]
    public string? Dba { get; set; }
    
    [JsonPropertyName("active")]
    public bool Active { get; set; }
    
    [JsonPropertyName("ivr_id")]
    public string? IvrId { get; set; }
    
    [JsonPropertyName("address1")]
    public string? Address1 { get; set; }
    
    [JsonPropertyName("city")]
    public string? City { get; set; }
    
    [JsonPropertyName("state")]
    public string? State { get; set; }
    
    [JsonPropertyName("postal_code")]
    public string? PostalCode { get; set; }
    
    [JsonPropertyName("phone")]
    public string? Phone { get; set; }
    
    [JsonPropertyName("email")]
    public string? Email { get; set; }
    
    [JsonPropertyName("cmms_prog")]
    public string? CmmsProg { get; set; }
    
    [JsonPropertyName("invoicing_method")]
    public string? InvoicingMethod { get; set; }
    
    [JsonPropertyName("taxable")]
    public bool? Taxable { get; set; }
}