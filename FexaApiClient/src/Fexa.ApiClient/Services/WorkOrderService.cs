using Microsoft.Extensions.Logging;
using Fexa.ApiClient.Models;
using System.Linq;
using System.Text.Json;
using System.Web;

namespace Fexa.ApiClient.Services;

public class WorkOrderService : IWorkOrderService
{
    private readonly IFexaApiService _apiService;
    private readonly ILogger<WorkOrderService> _logger;
    private const string BaseEndpoint = "/api/ev1/workorders";

    public WorkOrderService(IFexaApiService apiService, ILogger<WorkOrderService> logger)
    {
        _apiService = apiService;
        _logger = logger;
    }

    public async Task<PagedResponse<WorkOrder>> GetWorkOrdersAsync(QueryParameters? parameters = null)
    {
        _logger.LogDebug("Getting work orders with parameters: {Parameters}", parameters);
        
        var queryParams = parameters ?? new QueryParameters();
        
        // Build query string for pagination
        var paginationParams = new Dictionary<string, string>
        {
            ["start"] = queryParams.Start.ToString(),
            ["limit"] = queryParams.Limit.ToString()
        };
        
        if (!string.IsNullOrWhiteSpace(queryParams.SortBy))
            paginationParams["sortBy"] = queryParams.SortBy;
            
        if (queryParams.SortDescending)
            paginationParams["sortDesc"] = "true";
        
        // Add filters as URL-encoded JSON array if present
        if (queryParams.Filters?.Any() == true)
        {
            var filterArray = queryParams.Filters.Select(f => new Dictionary<string, object>
            {
                ["property"] = f.Property,
                ["operator"] = f.Operator ?? "equals",
                ["value"] = f.Value ?? ""
            }).ToList();
            
            var filterJson = JsonSerializer.Serialize(filterArray);
            var encodedFilter = HttpUtility.UrlEncode(filterJson);
            paginationParams["filters"] = encodedFilter;
            
            _logger.LogDebug("Generated filter JSON: {FilterJson}", filterJson);
        }
        
        var queryString = BuildQueryString(paginationParams);
        var endpoint = $"{BaseEndpoint}{queryString}";
        
        _logger.LogInformation("Final work order endpoint: {Endpoint}", endpoint);
        var response = await _apiService.GetAsync<WorkOrdersResponse>(endpoint);
        
        // Convert to PagedResponse format
        // Use pagination info if available, otherwise use the count of returned items
        var totalCount = response?.Pagination?.Total ?? response?.WorkOrders?.Count ?? 0;
        var currentPage = response?.Pagination?.CurrentPage ?? ((queryParams.Start / queryParams.Limit) + 1);
        var pageSize = response?.Pagination?.PerPage ?? queryParams.Limit;
        
        return new PagedResponse<WorkOrder>
        {
            Data = response?.WorkOrders,
            TotalCount = totalCount,
            Page = currentPage,
            PageSize = pageSize,
            TotalPages = totalCount > 0 
                ? (int)Math.Ceiling(totalCount / (double)pageSize) 
                : 0
        };
    }

    public async Task<WorkOrder> GetWorkOrderAsync(int id)
    {
        _logger.LogDebug("Getting work order with ID: {Id}", id);
        
        var endpoint = $"{BaseEndpoint}/{id}";
        
        // The API returns the work order directly under "workorders" key for single items
        var response = await _apiService.GetAsync<SingleWorkOrderResponse>(endpoint);
        
        if (response?.WorkOrder == null)
        {
            throw new InvalidOperationException($"Work order with ID {id} not found");
        }
        
        return response.WorkOrder;
    }

    public async Task<PagedResponse<WorkOrder>> GetWorkOrdersByStatusAsync(string status, QueryParameters? parameters = null)
    {
        _logger.LogDebug("Getting work orders with status: {Status}", status);
        
        var queryParams = parameters ?? new QueryParameters();
        queryParams.Filters = queryParams.Filters ?? new List<FexaFilter>();
        queryParams.Filters.Add(new FexaFilter(
            "object_state.status.name",
            status
        ));
        
        return await GetWorkOrdersAsync(queryParams);
    }

    public async Task<PagedResponse<WorkOrder>> GetWorkOrdersByVendorAsync(int vendorId, QueryParameters? parameters = null)
    {
        _logger.LogDebug("Getting work orders for vendor: {VendorId}", vendorId);
        
        var queryParams = parameters ?? new QueryParameters();
        queryParams.Filters = queryParams.Filters ?? new List<FexaFilter>();
        queryParams.Filters.Add(new FexaFilter(
            "vendors.id",  // Use vendors.id for vendor filtering
            vendorId
        ));
        
        return await GetWorkOrdersAsync(queryParams);
    }

    public async Task<PagedResponse<WorkOrder>> GetWorkOrdersByClientAsync(int clientId, QueryParameters? parameters = null)
    {
        _logger.LogDebug("Getting work orders for client: {ClientId}", clientId);
        
        var queryParams = parameters ?? new QueryParameters();
        queryParams.Filters = queryParams.Filters ?? new List<FexaFilter>();
        queryParams.Filters.Add(new FexaFilter(
            "clients.id",  // Use clients.id for client filtering
            clientId
        ));
        
        return await GetWorkOrdersAsync(queryParams);
    }

    public async Task<PagedResponse<WorkOrder>> GetWorkOrdersByTechnicianAsync(int technicianId, QueryParameters? parameters = null)
    {
        _logger.LogDebug("Getting work orders for technician: {TechnicianId}", technicianId);
        
        var queryParams = parameters ?? new QueryParameters();
        queryParams.Filters = queryParams.Filters ?? new List<FexaFilter>();
        queryParams.Filters.Add(new FexaFilter(
            "lead_technician_role_id",
            technicianId
        ));
        
        return await GetWorkOrdersAsync(queryParams);
    }

    public async Task<PagedResponse<WorkOrder>> GetWorkOrdersByDateRangeAsync(DateTime startDate, DateTime endDate, QueryParameters? parameters = null)
    {
        _logger.LogDebug("Getting work orders between {StartDate} and {EndDate}", startDate, endDate);
        
        var queryParams = parameters ?? new QueryParameters();
        queryParams.Filters = queryParams.Filters ?? new List<FexaFilter>();
        queryParams.Filters.Add(new FexaFilter(
            "created_at",
            new[] { 
                startDate.ToString("yyyy-MM-dd HH:mm:ss"), 
                endDate.ToString("yyyy-MM-dd HH:mm:ss") 
            },
            FilterOperators.Between
        ));
        
        return await GetWorkOrdersAsync(queryParams);
    }

    private string BuildQueryString(Dictionary<string, string> parameters)
    {
        if (!parameters.Any())
            return string.Empty;

        var queryParts = parameters
            .Where(p => !string.IsNullOrWhiteSpace(p.Value))
            .Select(p => $"{Uri.EscapeDataString(p.Key)}={Uri.EscapeDataString(p.Value)}");

        return "?" + string.Join("&", queryParts);
    }
    
    public async Task<List<WorkOrder>> GetAllWorkOrdersAsync(QueryParameters? baseParameters = null, int maxPages = 10)
    {
        _logger.LogInformation("Fetching all work orders (up to {MaxPages} pages)", maxPages);
        
        var allWorkOrders = new List<WorkOrder>();
        var pageSize = baseParameters?.Limit ?? 100; // Use 100 as default for bulk fetching
        var currentPage = 0;
        var hasMoreData = true;
        
        while (hasMoreData && currentPage < maxPages)
        {
            var parameters = new QueryParameters
            {
                Start = currentPage * pageSize,
                Limit = pageSize,
                SortBy = baseParameters?.SortBy,
                SortDescending = baseParameters?.SortDescending ?? false,
                Filters = baseParameters?.Filters
            };
            
            var response = await GetWorkOrdersAsync(parameters);
            
            if (response.Data != null && response.Data.Any())
            {
                allWorkOrders.AddRange(response.Data);
                _logger.LogDebug("Fetched page {Page} with {Count} work orders. Total so far: {Total}", 
                    currentPage + 1, response.Data.Count(), allWorkOrders.Count);
            }
            
            // Check if there are more pages
            hasMoreData = response.Data != null && 
                         response.Data.Count() == pageSize && 
                         (response.TotalCount == 0 || allWorkOrders.Count < response.TotalCount);
            
            currentPage++;
        }
        
        _logger.LogInformation("Fetched {Total} work orders across {Pages} pages", 
            allWorkOrders.Count, currentPage);
        
        return allWorkOrders;
    }
    
    public async Task<List<WorkOrder>> GetAllWorkOrdersByStatusAsync(string status, QueryParameters? baseParameters = null, int maxPages = 10)
    {
        _logger.LogInformation("Fetching all work orders with status '{Status}' (up to {MaxPages} pages)", status, maxPages);
        
        var allWorkOrders = new List<WorkOrder>();
        var pageSize = baseParameters?.Limit ?? 100; // Use 100 as default for bulk fetching
        var currentPage = 0;
        var hasMoreData = true;
        
        while (hasMoreData && currentPage < maxPages)
        {
            var parameters = new QueryParameters
            {
                Start = currentPage * pageSize,
                Limit = pageSize,
                SortBy = baseParameters?.SortBy,
                SortDescending = baseParameters?.SortDescending ?? false,
                Filters = baseParameters?.Filters
            };
            
            var response = await GetWorkOrdersByStatusAsync(status, parameters);
            
            if (response.Data != null && response.Data.Any())
            {
                // Only add work orders that actually match the requested status
                // since the API might return incorrect results
                var matchingOrders = response.Data.Where(w => w.Status == status).ToList();
                allWorkOrders.AddRange(matchingOrders);
                
                if (matchingOrders.Count != response.Data.Count())
                {
                    _logger.LogWarning("API returned {Total} work orders but only {Matching} had status '{Status}'", 
                        response.Data.Count(), matchingOrders.Count, status);
                }
                
                _logger.LogDebug("Fetched page {Page} with {Count} matching work orders. Total so far: {Total}", 
                    currentPage + 1, matchingOrders.Count, allWorkOrders.Count);
            }
            
            // Check if there are more pages
            hasMoreData = response.Data != null && 
                         response.Data.Count() == pageSize && 
                         (response.TotalCount == 0 || allWorkOrders.Count < response.TotalCount);
            
            currentPage++;
        }
        
        _logger.LogInformation("Fetched {Total} work orders with status '{Status}' across {Pages} pages", 
            allWorkOrders.Count, status, currentPage);
        
        return allWorkOrders;
    }
    
    public async Task<WorkOrder> UpdateStatusAsync(int workOrderId, int newStatusId, string? reason = null)
    {
        _logger.LogInformation("Updating work order {WorkOrderId} status to {StatusId}", workOrderId, newStatusId);
        
        // First, get the current work order to compare status before and after
        var originalWorkOrder = await GetWorkOrderAsync(workOrderId);
        var originalStatusId = originalWorkOrder.ObjectState?.StatusId ?? 0;
        
        // Try different possible endpoints for updating work order status
        // Based on Postman testing, the status ID should be in the URL path
        var possibleEndpoints = new[]
        {
            $"/api/ev1/workorders/{workOrderId}/update_status/{newStatusId}",  // Status ID in path
            $"/api/ev1/workorders/{workOrderId}/update_status",                // Status ID in body (fallback)
            $"/api/ev1/workorders/{workOrderId}/status/{newStatusId}",         // Alternative with ID in path
            $"/api/ev1/workorders/{workOrderId}/status",                       // Alternative with ID in body
            $"/api/v2/workorders/{workOrderId}/status/{newStatusId}",          // v2 with ID in path
            $"/api/ev1/workorders/{workOrderId}/transition"                    // Transition endpoint
        };
        
        // Some endpoints expect the status ID in the URL, others in the body
        var requestBody = new
        {
            status_id = newStatusId,
            reason = reason ?? string.Empty
        };
        
        Exception? lastException = null;
        
        foreach (var endpoint in possibleEndpoints)
        {
            try
            {
                _logger.LogDebug("Trying endpoint: {Endpoint}", endpoint);
                
                // For endpoints with status ID in the path, send no body at all
                // For other endpoints, send the status_id and reason in the body
                object? actualBody = endpoint.Contains($"/{newStatusId}") 
                    ? null  // No body for endpoints with ID in path
                    : requestBody;  // Body with status_id and reason for other endpoints
                var response = await _apiService.PutAsync<dynamic>(endpoint, actualBody);
                
                // Check if response indicates success or routing error
                if (response is JsonElement jsonResponse)
                {
                    var responseStr = jsonResponse.ToString();
                    if (!string.IsNullOrEmpty(responseStr))
                    {
                        // Check for explicit success:false or routing_error
                        if (responseStr.Contains("\"success\":false") || responseStr.Contains("routing_error"))
                        {
                            _logger.LogDebug("Endpoint {Endpoint} returned error: {Response}", endpoint, responseStr);
                            continue;
                        }
                        // If we have success:true, the update worked!
                        if (responseStr.Contains("\"success\":true"))
                        {
                            _logger.LogInformation("Status update successful via {Endpoint}. Response: {Response}", endpoint, responseStr);
                            // Get the updated work order to return
                            var successfullyUpdated = await GetWorkOrderAsync(workOrderId);
                            return successfullyUpdated;
                        }
                    }
                }
                else if (response != null)
                {
                    var responseStr = response?.ToString() ?? string.Empty;
                    if (!string.IsNullOrEmpty(responseStr) && (responseStr.Contains("\"success\":false") || responseStr.Contains("routing_error")))
                    {
                        _logger.LogDebug("Endpoint {Endpoint} failed: {Error}", endpoint, (string)responseStr);
                        continue;
                    }
                }
                
                // Verify the status actually changed
                var updatedWorkOrder = await GetWorkOrderAsync(workOrderId);
                var updatedStatusId = updatedWorkOrder.ObjectState?.StatusId ?? 0;
                
                if (updatedStatusId == newStatusId)
                {
                    _logger.LogInformation("Successfully updated work order {WorkOrderId} status from {OldStatus} to {NewStatus} using endpoint {Endpoint}", 
                        workOrderId, originalStatusId, newStatusId, endpoint);
                    return updatedWorkOrder;
                }
                else
                {
                    _logger.LogWarning("Endpoint {Endpoint} did not update status. Current status: {CurrentStatus}, Expected: {ExpectedStatus}", 
                        endpoint, updatedStatusId, newStatusId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogDebug("Endpoint {Endpoint} failed: {Message}", endpoint, ex.Message);
                lastException = ex;
            }
        }
        
        // If all endpoints failed, the API likely doesn't support direct work order status updates
        // Work orders may only allow status changes through assignment status changes
        _logger.LogWarning("All direct status update attempts failed for work order {WorkOrderId}", workOrderId);
        
        // Log what we tried
        _logger.LogInformation("Attempted endpoints for work order {WorkOrderId} with status {StatusId}", workOrderId, newStatusId);
        
        // Since work orders contain assignments, and assignments have their own statuses,
        // it's possible that work order status is computed from assignment statuses
        // and cannot be directly updated
        
        throw new InvalidOperationException(
            $"Unable to update work order {workOrderId} status directly. " +
            $"The Fexa API does not appear to support direct work order status updates. " +
            $"Work order status may be automatically determined by the status of its assignments. " +
            $"Current status remains: {originalWorkOrder.ObjectState?.Status?.Name} (ID: {originalWorkOrder.ObjectState?.StatusId}). " +
            $"To change work order status, you may need to update the status of its assignments instead.",
            lastException);
    }
    
    // Commented out: API doesn't support filtering on client_purchase_order_numbers
    // and we decided not to implement workarounds for now
    /*
    public async Task<List<WorkOrder>> GetWorkOrdersByClientPONumberAsync(string poNumber, int? vendorId = null, int? clientId = null)
    {
        _logger.LogInformation("Searching for work orders with Client PO Number: {PONumber}", poNumber);
        
        // Since the API doesn't support direct filtering on client_purchase_order_numbers,
        // we need to fetch work orders and filter in memory
        
        // Build initial filters for vendor/client if provided
        var filters = new FilterBuilder();
        if (vendorId.HasValue)
        {
            filters.WhereVendorId(vendorId.Value);
        }
        if (clientId.HasValue)
        {
            filters.WhereClientId(clientId.Value);
        }
        
        var parameters = new QueryParameters
        {
            Limit = 100,  // Get more records per page for efficiency
            Filters = filters.Build()
        };
        
        // Fetch all work orders (with vendor/client filter if provided)
        var allWorkOrders = await GetAllWorkOrdersAsync(parameters, maxPages: 50);
        
        // Filter in memory by PO number
        var matchingWorkOrders = new List<WorkOrder>();
        
        foreach (var workOrder in allWorkOrders)
        {
            if (workOrder.ClientPurchaseOrderNumbers != null && workOrder.ClientPurchaseOrderNumbers.Any())
            {
                // Check if any PO number matches (case-insensitive)
                var hasMatchingPO = workOrder.ClientPurchaseOrderNumbers
                    .Any(po => po.PurchaseOrderNumber?.Equals(poNumber, StringComparison.OrdinalIgnoreCase) == true);
                
                if (hasMatchingPO)
                {
                    matchingWorkOrders.Add(workOrder);
                    _logger.LogDebug("Found work order {WorkOrderId} with matching PO number", workOrder.Id);
                }
            }
        }
        
        _logger.LogInformation("Found {Count} work orders with Client PO Number '{PONumber}'", 
            matchingWorkOrders.Count, poNumber);
        
        return matchingWorkOrders;
    }
    */
}