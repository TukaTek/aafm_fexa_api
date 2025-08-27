using Fexa.ApiClient.Models;

namespace Fexa.ApiClient.Services;

public interface IWorkOrderService
{
    Task<PagedResponse<WorkOrder>> GetWorkOrdersAsync(QueryParameters? parameters = null);
    Task<WorkOrder> GetWorkOrderAsync(int id);
    Task<PagedResponse<WorkOrder>> GetWorkOrdersByStatusAsync(string status, QueryParameters? parameters = null);
    Task<PagedResponse<WorkOrder>> GetWorkOrdersByVendorAsync(int vendorId, QueryParameters? parameters = null);
    Task<PagedResponse<WorkOrder>> GetWorkOrdersByClientAsync(int clientId, QueryParameters? parameters = null);
    Task<PagedResponse<WorkOrder>> GetWorkOrdersByTechnicianAsync(int technicianId, QueryParameters? parameters = null);
    Task<PagedResponse<WorkOrder>> GetWorkOrdersByDateRangeAsync(DateTime startDate, DateTime endDate, QueryParameters? parameters = null);
    
    // Pagination helpers - fetch all pages
    Task<List<WorkOrder>> GetAllWorkOrdersAsync(QueryParameters? baseParameters = null, int maxPages = 10);
    Task<List<WorkOrder>> GetAllWorkOrdersByStatusAsync(string status, QueryParameters? baseParameters = null, int maxPages = 10);
    
    // Status management
    Task<WorkOrder> UpdateStatusAsync(int workOrderId, int newStatusId, string? reason = null);
    
    // Client PO search - Uses purchase_order_number filter
    Task<PagedResponse<WorkOrder>> GetWorkOrdersByClientPOAsync(string poNumber, QueryParameters? parameters = null);
    Task<List<WorkOrder>> GetAllWorkOrdersByClientPOAsync(string poNumber, QueryParameters? baseParameters = null, int maxPages = 10);
    
    // Create work order
    Task<WorkOrder> CreateWorkOrderAsync(CreateWorkOrderRequest request);
}