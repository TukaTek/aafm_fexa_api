using Fexa.ApiClient.Models;

namespace Fexa.ApiClient.Services;

public interface IVendorService
{
    Task<PagedResponse<Vendor>> GetVendorsAsync(QueryParameters? parameters = null, CancellationToken cancellationToken = default);
    Task<Vendor?> GetVendorAsync(int id, CancellationToken cancellationToken = default);
    
    // Pagination helper - fetch all pages
    Task<List<Vendor>> GetAllVendorsAsync(QueryParameters? baseParameters = null, int maxPages = 10, CancellationToken cancellationToken = default);
    
    // Additional vendor-specific queries
    Task<PagedResponse<Vendor>> GetActiveVendorsAsync(QueryParameters? parameters = null, CancellationToken cancellationToken = default);
    Task<PagedResponse<Vendor>> GetVendorsByComplianceStatusAsync(bool compliant, QueryParameters? parameters = null, CancellationToken cancellationToken = default);
    Task<PagedResponse<Vendor>> GetAssignableVendorsAsync(QueryParameters? parameters = null, CancellationToken cancellationToken = default);
    
    // Get vendors assigned to a work order
    Task<List<Vendor>> GetVendorsByWorkOrderIdAsync(int workOrderId, CancellationToken cancellationToken = default);
}