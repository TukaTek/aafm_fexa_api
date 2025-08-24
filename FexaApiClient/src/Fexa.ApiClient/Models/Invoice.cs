namespace Fexa.ApiClient.Models;

public class ClientInvoice
{
    public int Id { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int? WorkOrderId { get; set; }
    public int? VendorId { get; set; }
    
    // Additional properties can be added based on actual API response
}