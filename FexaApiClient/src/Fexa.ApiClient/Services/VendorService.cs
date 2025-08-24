using Microsoft.Extensions.Logging;
using Fexa.ApiClient.Models;
using System.Text.Json;
using System.Web;

namespace Fexa.ApiClient.Services;

public class VendorService : IVendorService
{
    private readonly IFexaApiService _apiService;
    private readonly IWorkOrderService _workOrderService;
    private readonly ILogger<VendorService> _logger;
    private const string BaseEndpoint = "/api/ev1/subcontractors";

    public VendorService(IFexaApiService apiService, IWorkOrderService workOrderService, ILogger<VendorService> logger)
    {
        _apiService = apiService ?? throw new ArgumentNullException(nameof(apiService));
        _workOrderService = workOrderService ?? throw new ArgumentNullException(nameof(workOrderService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<PagedResponse<Vendor>> GetVendorsAsync(QueryParameters? parameters = null, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching vendors with parameters: Start={Start}, Limit={Limit}", 
            parameters?.Start ?? 0, parameters?.Limit ?? 100);

        var queryParams = new Dictionary<string, string>
        {
            ["start"] = (parameters?.Start ?? 0).ToString(),
            ["limit"] = (parameters?.Limit ?? 100).ToString()
        };

        // Add sorting if specified
        if (!string.IsNullOrEmpty(parameters?.SortBy))
        {
            queryParams["sort"] = parameters.SortBy;
            if (parameters.SortDescending)
            {
                queryParams["sort_desc"] = "true";
            }
        }

        // Add filters if specified
        if (parameters?.Filters != null && parameters.Filters.Any())
        {
            var filterJson = JsonSerializer.Serialize(parameters.Filters.Select(f => new
            {
                property = f.Property,
                @operator = f.Operator,
                value = f.Value
            }));
            queryParams["filters"] = filterJson;
        }

        var queryString = string.Join("&", queryParams.Select(kvp => $"{kvp.Key}={HttpUtility.UrlEncode(kvp.Value)}"));
        var endpoint = $"{BaseEndpoint}?{queryString}";

        var response = await _apiService.GetAsync<VendorsResponse>(endpoint, cancellationToken);

        if (response?.Vendors == null)
        {
            _logger.LogWarning("Received null or empty response from vendors endpoint");
            return new PagedResponse<Vendor>
            {
                Data = new List<Vendor>(),
                TotalCount = 0,
                Page = 1,
                PageSize = parameters?.Limit ?? 100,
                Success = false,
                Message = "No data received from API"
            };
        }

        var limit = response.Limit > 0 ? response.Limit : (parameters?.Limit ?? 100);
        var totalPages = response.Total > 0 && limit > 0 ? (int)Math.Ceiling((double)response.Total / limit) : 0;
        var currentPage = limit > 0 ? (response.Start / limit) + 1 : 1;
        
        return new PagedResponse<Vendor>
        {
            Data = response.Vendors,
            TotalCount = response.Total,
            Page = currentPage,
            PageSize = limit,
            TotalPages = totalPages,
            Success = true
        };
    }

    public async Task<Vendor?> GetVendorAsync(int id, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching vendor with ID: {VendorId}", id);

        var endpoint = $"{BaseEndpoint}/{id}";
        
        try
        {
            var response = await _apiService.GetAsync<VendorResponse>(endpoint, cancellationToken);
            
            if (response?.Vendors == null || !response.Vendors.Any())
            {
                _logger.LogWarning("Vendor with ID {VendorId} not found or response was empty", id);
                return null;
            }

            // The API returns an array even for single vendor requests
            return response.Vendors.FirstOrDefault();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching vendor with ID {VendorId}", id);
            throw;
        }
    }

    public async Task<List<Vendor>> GetAllVendorsAsync(QueryParameters? baseParameters = null, int maxPages = 10, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching all vendors (up to {MaxPages} pages)", maxPages);
        
        var allVendors = new List<Vendor>();
        var currentPage = 0;
        var pageSize = baseParameters?.Limit ?? 100;
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
            
            var response = await GetVendorsAsync(parameters, cancellationToken);
            
            if (response.Data != null && response.Data.Any())
            {
                allVendors.AddRange(response.Data);
                _logger.LogDebug("Fetched page {Page} with {Count} vendors. Total so far: {Total}", 
                    currentPage + 1, response.Data.Count(), allVendors.Count);
            }
            
            hasMoreData = response.Data != null && 
                         response.Data.Count() == pageSize && 
                         (response.TotalCount == 0 || allVendors.Count < response.TotalCount);
            
            currentPage++;
        }
        
        _logger.LogInformation("Fetched {Total} vendors across {Pages} pages", 
            allVendors.Count, currentPage);
        
        return allVendors;
    }

    public async Task<PagedResponse<Vendor>> GetActiveVendorsAsync(QueryParameters? parameters = null, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching active vendors");
        
        var filters = new List<FexaFilter>(parameters?.Filters ?? new List<FexaFilter>())
        {
            new FexaFilter("active", true)
        };
        
        var queryParams = new QueryParameters
        {
            Start = parameters?.Start ?? 0,
            Limit = parameters?.Limit ?? 100,
            SortBy = parameters?.SortBy,
            SortDescending = parameters?.SortDescending ?? false,
            Filters = filters
        };
        
        return await GetVendorsAsync(queryParams, cancellationToken);
    }

    public async Task<PagedResponse<Vendor>> GetVendorsByComplianceStatusAsync(bool compliant, QueryParameters? parameters = null, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching vendors by compliance status: {Compliant}", compliant);
        
        var filters = new List<FexaFilter>(parameters?.Filters ?? new List<FexaFilter>())
        {
            new FexaFilter("compliance_requirement_met", compliant)
        };
        
        var queryParams = new QueryParameters
        {
            Start = parameters?.Start ?? 0,
            Limit = parameters?.Limit ?? 100,
            SortBy = parameters?.SortBy,
            SortDescending = parameters?.SortDescending ?? false,
            Filters = filters
        };
        
        return await GetVendorsAsync(queryParams, cancellationToken);
    }

    public async Task<PagedResponse<Vendor>> GetAssignableVendorsAsync(QueryParameters? parameters = null, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching assignable vendors");
        
        var filters = new List<FexaFilter>(parameters?.Filters ?? new List<FexaFilter>())
        {
            new FexaFilter("assignable", true)
        };
        
        var queryParams = new QueryParameters
        {
            Start = parameters?.Start ?? 0,
            Limit = parameters?.Limit ?? 100,
            SortBy = parameters?.SortBy,
            SortDescending = parameters?.SortDescending ?? false,
            Filters = filters
        };
        
        return await GetVendorsAsync(queryParams, cancellationToken);
    }

    public async Task<List<Vendor>> GetVendorsByWorkOrderIdAsync(int workOrderId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching vendors for work order ID: {WorkOrderId}", workOrderId);
        
        // Step 1: Get the work order with assignments
        var workOrder = await _workOrderService.GetWorkOrderAsync(workOrderId);
        
        if (workOrder?.Assignments == null || !workOrder.Assignments.Any())
        {
            _logger.LogInformation("No assignments found for work order {WorkOrderId}", workOrderId);
            return new List<Vendor>();
        }
        
        // Step 2: Extract unique vendor IDs from assignments (role_id)
        var vendorIds = workOrder.Assignments
            .Where(a => a.RoleId.HasValue)
            .Select(a => a.RoleId!.Value)
            .Distinct()
            .ToList();
        
        if (!vendorIds.Any())
        {
            _logger.LogInformation("No vendor IDs found in assignments for work order {WorkOrderId}", workOrderId);
            return new List<Vendor>();
        }
        
        _logger.LogInformation("Found {Count} unique vendor(s) in work order {WorkOrderId}: {VendorIds}", 
            vendorIds.Count, workOrderId, string.Join(", ", vendorIds));
        
        // Step 3: Fetch each vendor by ID
        var vendors = new List<Vendor>();
        foreach (var vendorId in vendorIds)
        {
            try
            {
                var vendor = await GetVendorAsync(vendorId, cancellationToken);
                if (vendor != null)
                {
                    vendors.Add(vendor);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to fetch vendor {VendorId} for work order {WorkOrderId}", 
                    vendorId, workOrderId);
            }
        }
        
        _logger.LogInformation("Successfully fetched {Count} vendor(s) for work order {WorkOrderId}", 
            vendors.Count, workOrderId);
        
        return vendors;
    }
}