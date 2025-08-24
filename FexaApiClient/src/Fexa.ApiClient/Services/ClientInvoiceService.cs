using Microsoft.Extensions.Logging;
using Fexa.ApiClient.Models;

namespace Fexa.ApiClient.Services;

public class ClientInvoiceService : IClientInvoiceService
{
    private readonly IFexaApiService _apiService;
    private readonly ILogger<ClientInvoiceService> _logger;
    private const string InvoicesEndpoint = "/api/ev1/client_invoices";
    
    public ClientInvoiceService(IFexaApiService apiService, ILogger<ClientInvoiceService> logger)
    {
        _apiService = apiService ?? throw new ArgumentNullException(nameof(apiService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
    
    public async Task<PagedResponse<ClientInvoice>> GetInvoicesAsync(
        QueryParameters? parameters = null, 
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting invoices with parameters: {@Parameters}", parameters);
        
        var queryString = BuildQueryString(parameters);
        var endpoint = string.IsNullOrEmpty(queryString) ? InvoicesEndpoint : $"{InvoicesEndpoint}?{queryString}";
        
        return await _apiService.GetAsync<PagedResponse<ClientInvoice>>(endpoint, cancellationToken);
    }
    
    public async Task<ClientInvoice> GetInvoiceAsync(int invoiceId, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting invoice with ID: {InvoiceId}", invoiceId);
        
        var response = await _apiService.GetAsync<BaseResponse<ClientInvoice>>(
            $"{InvoicesEndpoint}/{invoiceId}", 
            cancellationToken);
            
        return response.Data ?? throw new InvalidOperationException("Invoice not found");
    }
    
    public async Task<PagedResponse<ClientInvoice>> GetInvoicesByWorkOrderAsync(
        int workOrderId, 
        QueryParameters? parameters = null, 
        CancellationToken cancellationToken = default)
    {
        parameters ??= new QueryParameters();
        
        // Add workorder filter to existing parameters
        var filters = parameters.Filters?.ToList() ?? new List<FexaFilter>();
        filters.Add(new FexaFilter("workorders.id", workOrderId));
        parameters.Filters = filters;
        
        _logger.LogDebug("Getting invoices for workorder {WorkOrderId}", workOrderId);
        
        return await GetInvoicesAsync(parameters, cancellationToken);
    }
    
    public async Task<PagedResponse<ClientInvoice>> GetInvoicesByWorkOrdersAsync(
        int[] workOrderIds, 
        QueryParameters? parameters = null, 
        CancellationToken cancellationToken = default)
    {
        if (workOrderIds == null || workOrderIds.Length == 0)
            throw new ArgumentException("At least one work order ID must be provided", nameof(workOrderIds));
            
        parameters ??= new QueryParameters();
        
        // Add workorders filter using IN operator
        var filters = parameters.Filters?.ToList() ?? new List<FexaFilter>();
        filters.Add(new FexaFilter("workorders.id", workOrderIds, FilterOperators.In));
        parameters.Filters = filters;
        
        _logger.LogDebug("Getting invoices for workorders {WorkOrderIds}", workOrderIds);
        
        return await GetInvoicesAsync(parameters, cancellationToken);
    }
    
    public async Task<PagedResponse<ClientInvoice>> GetInvoicesByVendorAsync(
        int vendorId, 
        QueryParameters? parameters = null, 
        CancellationToken cancellationToken = default)
    {
        parameters ??= new QueryParameters();
        
        // Add vendor filter to existing parameters
        var filters = parameters.Filters?.ToList() ?? new List<FexaFilter>();
        filters.Add(new FexaFilter("vendors.id", vendorId));
        parameters.Filters = filters;
        
        _logger.LogDebug("Getting invoices for vendor {VendorId}", vendorId);
        
        return await GetInvoicesAsync(parameters, cancellationToken);
    }
    
    private string BuildQueryString(QueryParameters? parameters)
    {
        if (parameters == null)
            return string.Empty;
            
        var queryDict = parameters.ToDictionary();
        return string.Join("&", queryDict.Select(kvp => $"{kvp.Key}={Uri.EscapeDataString(kvp.Value)}"));
    }
}