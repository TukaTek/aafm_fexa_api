using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Fexa.ApiClient.Models;

namespace Fexa.ApiClient.Services;

/// <summary>
/// Alternative implementation of VisitService that allows configurable endpoint paths
/// </summary>
public class ConfigurableVisitService : IVisitService
{
    private readonly IFexaApiService _apiService;
    private readonly ILogger<ConfigurableVisitService> _logger;
    private readonly string _visitsEndpoint;
    
    public ConfigurableVisitService(
        IFexaApiService apiService, 
        ILogger<ConfigurableVisitService> logger,
        IConfiguration configuration)
    {
        _apiService = apiService ?? throw new ArgumentNullException(nameof(apiService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        
        // Allow override of the visits endpoint via configuration
        // Default to /api/v2/visits if not specified
        _visitsEndpoint = configuration["FexaApi:VisitsEndpoint"] ?? "/api/v2/visits";
        
        _logger.LogInformation("ConfigurableVisitService initialized with endpoint: {Endpoint}", _visitsEndpoint);
    }
    
    public async Task<PagedResponse<Visit>> GetVisitsAsync(
        QueryParameters? parameters = null, 
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting visits with parameters: {@Parameters}", parameters);
        _logger.LogDebug("Using endpoint: {Endpoint}", _visitsEndpoint);
        
        var queryString = BuildQueryString(parameters);
        var endpoint = string.IsNullOrEmpty(queryString) ? _visitsEndpoint : $"{_visitsEndpoint}?{queryString}";
        
        try
        {
            var response = await _apiService.GetAsync<PagedResponse<Visit>>(endpoint, cancellationToken);
            _logger.LogDebug("Successfully retrieved {Count} visits", response.Data?.Count() ?? 0);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get visits from endpoint {Endpoint}", endpoint);
            throw;
        }
    }
    
    public async Task<Visit> GetVisitAsync(int visitId, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting visit with ID: {VisitId}", visitId);
        
        var response = await _apiService.GetAsync<BaseResponse<Visit>>(
            $"{_visitsEndpoint}/{visitId}", 
            cancellationToken);
            
        return response.Data ?? throw new InvalidOperationException("Visit not found");
    }
    
    public async Task<PagedResponse<Visit>> GetVisitsByWorkOrderAsync(
        int workOrderId, 
        QueryParameters? parameters = null, 
        CancellationToken cancellationToken = default)
    {
        parameters ??= new QueryParameters();
        
        var filters = parameters.Filters?.ToList() ?? new List<FexaFilter>();
        filters.Add(new FexaFilter("workorders.id", workOrderId));
        parameters.Filters = filters;
        
        _logger.LogDebug("Getting visits for workorder {WorkOrderId}", workOrderId);
        
        return await GetVisitsAsync(parameters, cancellationToken);
    }
    
    public async Task<PagedResponse<Visit>> GetVisitsByWorkOrdersAsync(
        int[] workOrderIds, 
        QueryParameters? parameters = null, 
        CancellationToken cancellationToken = default)
    {
        if (workOrderIds == null || workOrderIds.Length == 0)
            throw new ArgumentException("At least one work order ID must be provided", nameof(workOrderIds));
            
        parameters ??= new QueryParameters();
        
        var filters = parameters.Filters?.ToList() ?? new List<FexaFilter>();
        filters.Add(new FexaFilter("workorders.id", workOrderIds, FilterOperators.In));
        parameters.Filters = filters;
        
        _logger.LogDebug("Getting visits for workorders {WorkOrderIds}", workOrderIds);
        
        return await GetVisitsAsync(parameters, cancellationToken);
    }
    
    public async Task<PagedResponse<Visit>> GetVisitsByTechnicianAsync(
        int technicianId, 
        QueryParameters? parameters = null, 
        CancellationToken cancellationToken = default)
    {
        parameters ??= new QueryParameters();
        
        var filters = parameters.Filters?.ToList() ?? new List<FexaFilter>();
        filters.Add(new FexaFilter("technicians.id", technicianId));
        parameters.Filters = filters;
        
        _logger.LogDebug("Getting visits for technician {TechnicianId}", technicianId);
        
        return await GetVisitsAsync(parameters, cancellationToken);
    }
    
    public async Task<PagedResponse<Visit>> GetVisitsByClientAsync(
        int clientId, 
        QueryParameters? parameters = null, 
        CancellationToken cancellationToken = default)
    {
        parameters ??= new QueryParameters();
        
        var filters = parameters.Filters?.ToList() ?? new List<FexaFilter>();
        filters.Add(new FexaFilter("clients.id", clientId));
        parameters.Filters = filters;
        
        _logger.LogDebug("Getting visits for client {ClientId}", clientId);
        
        return await GetVisitsAsync(parameters, cancellationToken);
    }
    
    public async Task<PagedResponse<Visit>> GetVisitsByLocationAsync(
        int locationId, 
        QueryParameters? parameters = null, 
        CancellationToken cancellationToken = default)
    {
        parameters ??= new QueryParameters();
        
        var filters = parameters.Filters?.ToList() ?? new List<FexaFilter>();
        filters.Add(new FexaFilter("locations.id", locationId));
        parameters.Filters = filters;
        
        _logger.LogDebug("Getting visits for location {LocationId}", locationId);
        
        return await GetVisitsAsync(parameters, cancellationToken);
    }
    
    public async Task<PagedResponse<Visit>> GetVisitsByStatusAsync(
        string status, 
        QueryParameters? parameters = null, 
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(status))
            throw new ArgumentException("Status cannot be empty", nameof(status));
            
        parameters ??= new QueryParameters();
        
        var filters = parameters.Filters?.ToList() ?? new List<FexaFilter>();
        filters.Add(new FexaFilter("visits.status", status));
        parameters.Filters = filters;
        
        _logger.LogDebug("Getting visits with status {Status}", status);
        
        return await GetVisitsAsync(parameters, cancellationToken);
    }
    
    public async Task<PagedResponse<Visit>> GetVisitsByDateRangeAsync(
        DateTime startDate, 
        DateTime endDate, 
        QueryParameters? parameters = null, 
        CancellationToken cancellationToken = default)
    {
        if (endDate < startDate)
            throw new ArgumentException("End date must be after start date", nameof(endDate));
            
        parameters ??= new QueryParameters();
        
        var filters = parameters.Filters?.ToList() ?? new List<FexaFilter>();
        filters.Add(new FexaFilter(
            "visits.scheduled_date", 
            new[] { startDate.ToString("yyyy-MM-dd"), endDate.ToString("yyyy-MM-dd") }, 
            FilterOperators.Between));
        parameters.Filters = filters;
        
        _logger.LogDebug("Getting visits between {StartDate} and {EndDate}", startDate, endDate);
        
        return await GetVisitsAsync(parameters, cancellationToken);
    }
    
    public async Task<PagedResponse<Visit>> GetVisitsByScheduledDateAsync(
        DateTime date, 
        QueryParameters? parameters = null, 
        CancellationToken cancellationToken = default)
    {
        parameters ??= new QueryParameters();
        
        var filters = parameters.Filters?.ToList() ?? new List<FexaFilter>();
        var dateStr = date.ToString("yyyy-MM-dd");
        filters.Add(new FexaFilter(
            "visits.scheduled_date", 
            new[] { dateStr, dateStr }, 
            FilterOperators.Between));
        parameters.Filters = filters;
        
        _logger.LogDebug("Getting visits scheduled on {Date}", date);
        
        return await GetVisitsAsync(parameters, cancellationToken);
    }
    
    public async Task<PagedResponse<Visit>> GetVisitsByActualDateAsync(
        DateTime date, 
        QueryParameters? parameters = null, 
        CancellationToken cancellationToken = default)
    {
        parameters ??= new QueryParameters();
        
        var filters = parameters.Filters?.ToList() ?? new List<FexaFilter>();
        var dateStr = date.ToString("yyyy-MM-dd");
        filters.Add(new FexaFilter(
            "visits.actual_date", 
            new[] { dateStr, dateStr }, 
            FilterOperators.Between));
        parameters.Filters = filters;
        
        _logger.LogDebug("Getting visits that occurred on {Date}", date);
        
        return await GetVisitsAsync(parameters, cancellationToken);
    }
    
    public async Task<PagedResponse<Visit>> GetVisitsScheduledAfterAsync(
        DateTime date, 
        QueryParameters? parameters = null, 
        CancellationToken cancellationToken = default)
    {
        parameters ??= new QueryParameters();
        
        var filters = parameters.Filters?.ToList() ?? new List<FexaFilter>();
        filters.Add(new FexaFilter(
            "visits.scheduled_date", 
            new[] { date.ToString("yyyy-MM-dd"), "2099-12-31" }, 
            FilterOperators.Between));
        parameters.Filters = filters;
        
        _logger.LogDebug("Getting visits scheduled after {Date}", date);
        
        return await GetVisitsAsync(parameters, cancellationToken);
    }
    
    public async Task<PagedResponse<Visit>> GetVisitsScheduledBeforeAsync(
        DateTime date, 
        QueryParameters? parameters = null, 
        CancellationToken cancellationToken = default)
    {
        parameters ??= new QueryParameters();
        
        var filters = parameters.Filters?.ToList() ?? new List<FexaFilter>();
        filters.Add(new FexaFilter(
            "visits.scheduled_date", 
            new[] { "1900-01-01", date.ToString("yyyy-MM-dd") }, 
            FilterOperators.Between));
        parameters.Filters = filters;
        
        _logger.LogDebug("Getting visits scheduled before {Date}", date);
        
        return await GetVisitsAsync(parameters, cancellationToken);
    }
    
    private string BuildQueryString(QueryParameters? parameters)
    {
        if (parameters == null)
            return string.Empty;
            
        var queryDict = parameters.ToDictionary();
        return string.Join("&", queryDict.Select(kvp => $"{kvp.Key}={Uri.EscapeDataString(kvp.Value)}"));
    }
    
    public async Task<List<Visit>> GetAllVisitsAsync(
        QueryParameters? baseParameters = null, 
        int maxPages = 10,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching all visits (up to {MaxPages} pages)", maxPages);
        
        var allVisits = new List<Visit>();
        var pageSize = baseParameters?.Limit ?? 100;
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
            
            var response = await GetVisitsAsync(parameters, cancellationToken);
            
            if (response.Data != null && response.Data.Any())
            {
                allVisits.AddRange(response.Data);
                _logger.LogDebug("Fetched page {Page} with {Count} visits. Total so far: {Total}", 
                    currentPage + 1, response.Data.Count(), allVisits.Count);
            }
            
            hasMoreData = response.Data != null && 
                         response.Data.Count() == pageSize && 
                         (response.TotalCount == 0 || allVisits.Count < response.TotalCount);
            
            currentPage++;
        }
        
        _logger.LogInformation("Fetched {Total} visits across {Pages} pages", 
            allVisits.Count, currentPage);
        
        return allVisits;
    }
    
    public async Task<List<Visit>> GetAllVisitsByWorkOrderAsync(
        int workOrderId, 
        QueryParameters? baseParameters = null, 
        int maxPages = 10,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching all visits for work order {WorkOrderId} (up to {MaxPages} pages)", workOrderId, maxPages);
        
        var allVisits = new List<Visit>();
        var pageSize = baseParameters?.Limit ?? 100;
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
            
            var response = await GetVisitsByWorkOrderAsync(workOrderId, parameters, cancellationToken);
            
            if (response.Data != null && response.Data.Any())
            {
                allVisits.AddRange(response.Data);
                _logger.LogDebug("Fetched page {Page} with {Count} visits. Total so far: {Total}", 
                    currentPage + 1, response.Data.Count(), allVisits.Count);
            }
            
            hasMoreData = response.Data != null && 
                         response.Data.Count() == pageSize && 
                         (response.TotalCount == 0 || allVisits.Count < response.TotalCount);
            
            currentPage++;
        }
        
        _logger.LogInformation("Fetched {Total} visits for work order {WorkOrderId} across {Pages} pages", 
            allVisits.Count, workOrderId, currentPage);
        
        return allVisits;
    }
    
    public async Task<List<Visit>> GetAllVisitsByDateRangeAsync(
        DateTime startDate, 
        DateTime endDate, 
        QueryParameters? baseParameters = null, 
        int maxPages = 10,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching all visits between {StartDate} and {EndDate} (up to {MaxPages} pages)", 
            startDate, endDate, maxPages);
        
        var allVisits = new List<Visit>();
        var pageSize = baseParameters?.Limit ?? 100;
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
            
            var response = await GetVisitsByDateRangeAsync(startDate, endDate, parameters, cancellationToken);
            
            if (response.Data != null && response.Data.Any())
            {
                allVisits.AddRange(response.Data);
                _logger.LogDebug("Fetched page {Page} with {Count} visits. Total so far: {Total}", 
                    currentPage + 1, response.Data.Count(), allVisits.Count);
            }
            
            hasMoreData = response.Data != null && 
                         response.Data.Count() == pageSize && 
                         (response.TotalCount == 0 || allVisits.Count < response.TotalCount);
            
            currentPage++;
        }
        
        _logger.LogInformation("Fetched {Total} visits between {StartDate} and {EndDate} across {Pages} pages", 
            allVisits.Count, startDate, endDate, currentPage);
        
        return allVisits;
    }
}