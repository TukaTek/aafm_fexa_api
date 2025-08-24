using Fexa.ApiClient.Models;

namespace Fexa.ApiClient.Services;

public interface IVisitService
{
    Task<PagedResponse<Visit>> GetVisitsAsync(
        QueryParameters? parameters = null, 
        CancellationToken cancellationToken = default);
    
    Task<Visit> GetVisitAsync(
        int visitId, 
        CancellationToken cancellationToken = default);
    
    Task<PagedResponse<Visit>> GetVisitsByWorkOrderAsync(
        int workOrderId, 
        QueryParameters? parameters = null, 
        CancellationToken cancellationToken = default);
    
    Task<PagedResponse<Visit>> GetVisitsByWorkOrdersAsync(
        int[] workOrderIds, 
        QueryParameters? parameters = null, 
        CancellationToken cancellationToken = default);
    
    Task<PagedResponse<Visit>> GetVisitsByTechnicianAsync(
        int technicianId, 
        QueryParameters? parameters = null, 
        CancellationToken cancellationToken = default);
    
    Task<PagedResponse<Visit>> GetVisitsByClientAsync(
        int clientId, 
        QueryParameters? parameters = null, 
        CancellationToken cancellationToken = default);
    
    Task<PagedResponse<Visit>> GetVisitsByLocationAsync(
        int locationId, 
        QueryParameters? parameters = null, 
        CancellationToken cancellationToken = default);
    
    Task<PagedResponse<Visit>> GetVisitsByStatusAsync(
        string status, 
        QueryParameters? parameters = null, 
        CancellationToken cancellationToken = default);
    
    Task<PagedResponse<Visit>> GetVisitsByDateRangeAsync(
        DateTime startDate, 
        DateTime endDate, 
        QueryParameters? parameters = null, 
        CancellationToken cancellationToken = default);
    
    Task<PagedResponse<Visit>> GetVisitsByScheduledDateAsync(
        DateTime date, 
        QueryParameters? parameters = null, 
        CancellationToken cancellationToken = default);
    
    Task<PagedResponse<Visit>> GetVisitsByActualDateAsync(
        DateTime date, 
        QueryParameters? parameters = null, 
        CancellationToken cancellationToken = default);
    
    Task<PagedResponse<Visit>> GetVisitsScheduledAfterAsync(
        DateTime date, 
        QueryParameters? parameters = null, 
        CancellationToken cancellationToken = default);
    
    Task<PagedResponse<Visit>> GetVisitsScheduledBeforeAsync(
        DateTime date, 
        QueryParameters? parameters = null, 
        CancellationToken cancellationToken = default);
    
    // Pagination helpers - fetch all pages
    Task<List<Visit>> GetAllVisitsAsync(
        QueryParameters? baseParameters = null, 
        int maxPages = 10,
        CancellationToken cancellationToken = default);
    
    Task<List<Visit>> GetAllVisitsByWorkOrderAsync(
        int workOrderId, 
        QueryParameters? baseParameters = null, 
        int maxPages = 10,
        CancellationToken cancellationToken = default);
    
    Task<List<Visit>> GetAllVisitsByDateRangeAsync(
        DateTime startDate, 
        DateTime endDate, 
        QueryParameters? baseParameters = null, 
        int maxPages = 10,
        CancellationToken cancellationToken = default);
}