using Microsoft.Extensions.Logging;
using Fexa.ApiClient.Models;
using System.Linq;
using System.Collections.Generic;
using System.Web;

namespace Fexa.ApiClient.Services;

public class VisitService : IVisitService
{
    private readonly IFexaApiService _apiService;
    private readonly ILogger<VisitService> _logger;
    // Note: The actual endpoint may vary. Common possibilities:
    // - /api/ev1/visits
    // - /api/v2/visits
    // - /api/ev1/appointments
    // - /api/ev1/service_visits
    // - /api/ev1/work_visits
    private const string VisitsEndpoint = "/api/ev1/visits";
    
    public VisitService(IFexaApiService apiService, ILogger<VisitService> logger)
    {
        _apiService = apiService ?? throw new ArgumentNullException(nameof(apiService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
    
    public async Task<PagedResponse<Visit>> GetVisitsAsync(
        QueryParameters? parameters = null, 
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting visits with parameters: {@Parameters}", parameters);
        _logger.LogDebug("Using endpoint: {Endpoint}", VisitsEndpoint);
        
        // Visits endpoint doesn't support filters array, only direct query params
        var visitParams = new VisitQueryParameters
        {
            Start = parameters?.Start ?? 0,
            Limit = parameters?.Limit ?? 20
        };
        
        var queryString = BuildVisitQueryString(visitParams);
        var endpoint = string.IsNullOrEmpty(queryString) ? VisitsEndpoint : $"{VisitsEndpoint}?{queryString}";
        
        // Get the raw response from Fexa API
        var response = await _apiService.GetAsync<VisitsResponse>(endpoint, cancellationToken);
        
        // Convert to our PagedResponse format
        return ConvertToPagedResponse(response, visitParams.Start, visitParams.Limit);
    }
    
    public async Task<Visit> GetVisitAsync(int visitId, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting visit with ID: {VisitId}", visitId);
        
        var response = await _apiService.GetAsync<BaseResponse<Visit>>(
            $"{VisitsEndpoint}/{visitId}", 
            cancellationToken);
            
        return response.Data ?? throw new InvalidOperationException("Visit not found");
    }
    
    public async Task<PagedResponse<Visit>> GetVisitsByWorkOrderAsync(
        int workOrderId, 
        QueryParameters? parameters = null, 
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting visits for workorder {WorkOrderId}", workOrderId);
        
        // Visits endpoint uses direct query params, not filters
        var visitParams = new VisitQueryParameters
        {
            Start = parameters?.Start ?? 0,
            Limit = parameters?.Limit ?? 20,
            WorkorderId = workOrderId
        };
        
        var queryString = BuildVisitQueryString(visitParams);
        var endpoint = string.IsNullOrEmpty(queryString) ? VisitsEndpoint : $"{VisitsEndpoint}?{queryString}";
        
        // Get the raw response from Fexa API
        var response = await _apiService.GetAsync<VisitsResponse>(endpoint, cancellationToken);
        
        // Convert to our PagedResponse format
        return ConvertToPagedResponse(response, visitParams.Start, visitParams.Limit);
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
        _logger.LogDebug("Getting visits for technician {TechnicianId}", technicianId);
        
        var visitParams = new VisitQueryParameters
        {
            Start = parameters?.Start ?? 0,
            Limit = parameters?.Limit ?? 20,
            TechnicianId = technicianId
        };
        
        var queryString = BuildVisitQueryString(visitParams);
        var endpoint = $"{VisitsEndpoint}?{queryString}";
        
        var response = await _apiService.GetAsync<VisitsResponse>(endpoint, cancellationToken);
        return ConvertToPagedResponse(response, visitParams.Start, visitParams.Limit);
    }
    
    public async Task<PagedResponse<Visit>> GetVisitsByClientAsync(
        int clientId, 
        QueryParameters? parameters = null, 
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting visits for client {ClientId}", clientId);
        
        var visitParams = new VisitQueryParameters
        {
            Start = parameters?.Start ?? 0,
            Limit = parameters?.Limit ?? 20,
            ClientId = clientId
        };
        
        var queryString = BuildVisitQueryString(visitParams);
        var endpoint = $"{VisitsEndpoint}?{queryString}";
        
        var response = await _apiService.GetAsync<VisitsResponse>(endpoint, cancellationToken);
        return ConvertToPagedResponse(response, visitParams.Start, visitParams.Limit);
    }
    
    public async Task<PagedResponse<Visit>> GetVisitsByLocationAsync(
        int locationId, 
        QueryParameters? parameters = null, 
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting visits for location {LocationId}", locationId);
        
        var visitParams = new VisitQueryParameters
        {
            Start = parameters?.Start ?? 0,
            Limit = parameters?.Limit ?? 20,
            LocationId = locationId
        };
        
        var queryString = BuildVisitQueryString(visitParams);
        var endpoint = $"{VisitsEndpoint}?{queryString}";
        
        var response = await _apiService.GetAsync<VisitsResponse>(endpoint, cancellationToken);
        return ConvertToPagedResponse(response, visitParams.Start, visitParams.Limit);
    }
    
    public async Task<PagedResponse<Visit>> GetVisitsByStatusAsync(
        string status, 
        QueryParameters? parameters = null, 
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(status))
            throw new ArgumentException("Status cannot be empty", nameof(status));
            
        _logger.LogDebug("Getting visits with status {Status}", status);
        
        var visitParams = new VisitQueryParameters
        {
            Start = parameters?.Start ?? 0,
            Limit = parameters?.Limit ?? 20,
            Status = status
        };
        
        var queryString = BuildVisitQueryString(visitParams);
        var endpoint = $"{VisitsEndpoint}?{queryString}";
        
        var response = await _apiService.GetAsync<VisitsResponse>(endpoint, cancellationToken);
        return ConvertToPagedResponse(response, visitParams.Start, visitParams.Limit);
    }
    
    public async Task<PagedResponse<Visit>> GetVisitsByDateRangeAsync(
        DateTime startDate, 
        DateTime endDate, 
        QueryParameters? parameters = null, 
        CancellationToken cancellationToken = default)
    {
        if (endDate < startDate)
            throw new ArgumentException("End date must be after start date", nameof(endDate));
            
        _logger.LogDebug("Getting visits between {StartDate} and {EndDate}", startDate, endDate);
        
        // Use start of startDate and end of endDate for inclusive range
        var startDateTime = startDate.Date; // 00:00:00
        var endDateTime = endDate.Date.AddDays(1).AddSeconds(-1); // 23:59:59
        
        // Build filter array for date range (use "start_date" without prefix)
        var filter = new[]
        {
            new Dictionary<string, object>
            {
                ["property"] = "start_date",
                ["operator"] = "between",
                ["value"] = new[] { 
                    startDateTime.ToString("yyyy-MM-dd HH:mm:ss"), 
                    endDateTime.ToString("yyyy-MM-dd HH:mm:ss") 
                }
            }
        };
        
        var filterJson = System.Text.Json.JsonSerializer.Serialize(filter);
        var encodedFilter = HttpUtility.UrlEncode(filterJson);
        
        var start = parameters?.Start ?? 0;
        var limit = parameters?.Limit ?? 20;
        var endpoint = $"{VisitsEndpoint}?start={start}&limit={limit}&filters={encodedFilter}";
        
        var response = await _apiService.GetAsync<VisitsResponse>(endpoint, cancellationToken);
        return ConvertToPagedResponse(response, start, limit);
    }
    
    public async Task<PagedResponse<Visit>> GetVisitsByScheduledDateAsync(
        DateTime date, 
        QueryParameters? parameters = null, 
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting visits scheduled on {Date}", date);
        
        // Build filter for single day using datetime range (00:00:00 to 23:59:59)
        var startOfDay = date.Date;
        var endOfDay = date.Date.AddDays(1).AddSeconds(-1);
        
        var filter = new[]
        {
            new Dictionary<string, object>
            {
                ["property"] = "start_date",
                ["operator"] = "between",
                ["value"] = new[] { 
                    startOfDay.ToString("yyyy-MM-dd HH:mm:ss"), 
                    endOfDay.ToString("yyyy-MM-dd HH:mm:ss") 
                }
            }
        };
        
        var filterJson = System.Text.Json.JsonSerializer.Serialize(filter);
        var encodedFilter = HttpUtility.UrlEncode(filterJson);
        
        var start = parameters?.Start ?? 0;
        var limit = parameters?.Limit ?? 20;
        var endpoint = $"{VisitsEndpoint}?start={start}&limit={limit}&filters={encodedFilter}";
        
        var response = await _apiService.GetAsync<VisitsResponse>(endpoint, cancellationToken);
        return ConvertToPagedResponse(response, start, limit);
    }
    
    public async Task<PagedResponse<Visit>> GetVisitsByActualDateAsync(
        DateTime date, 
        QueryParameters? parameters = null, 
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting visits with check-in on {Date}", date);
        
        // Build filter for single day using datetime range for check_in_time
        var startOfDay = date.Date;
        var endOfDay = date.Date.AddDays(1).AddSeconds(-1);
        
        var filter = new[]
        {
            new Dictionary<string, object>
            {
                ["property"] = "check_in_time",
                ["operator"] = "between",
                ["value"] = new[] { 
                    startOfDay.ToString("yyyy-MM-dd HH:mm:ss"), 
                    endOfDay.ToString("yyyy-MM-dd HH:mm:ss") 
                }
            }
        };
        
        var filterJson = System.Text.Json.JsonSerializer.Serialize(filter);
        var encodedFilter = HttpUtility.UrlEncode(filterJson);
        
        var start = parameters?.Start ?? 0;
        var limit = parameters?.Limit ?? 20;
        var endpoint = $"{VisitsEndpoint}?start={start}&limit={limit}&filters={encodedFilter}";
        
        var response = await _apiService.GetAsync<VisitsResponse>(endpoint, cancellationToken);
        return ConvertToPagedResponse(response, start, limit);
    }
    
    public async Task<PagedResponse<Visit>> GetVisitsScheduledAfterAsync(
        DateTime date, 
        QueryParameters? parameters = null, 
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting visits scheduled after {Date}", date);
        
        // For "after" a date, start from the beginning of the NEXT day
        var startDateTime = date.Date.AddDays(1); // Next day at 00:00:00
        
        // Build filter for visits after a specific date
        var filter = new[]
        {
            new Dictionary<string, object>
            {
                ["property"] = "start_date",
                ["operator"] = "between",
                ["value"] = new[] { 
                    startDateTime.ToString("yyyy-MM-dd HH:mm:ss"), 
                    "2099-12-31 23:59:59" 
                }
            }
        };
        
        var filterJson = System.Text.Json.JsonSerializer.Serialize(filter);
        var encodedFilter = HttpUtility.UrlEncode(filterJson);
        
        var start = parameters?.Start ?? 0;
        var limit = parameters?.Limit ?? 20;
        var endpoint = $"{VisitsEndpoint}?start={start}&limit={limit}&filters={encodedFilter}";
        
        var response = await _apiService.GetAsync<VisitsResponse>(endpoint, cancellationToken);
        return ConvertToPagedResponse(response, start, limit);
    }
    
    public async Task<PagedResponse<Visit>> GetVisitsScheduledBeforeAsync(
        DateTime date, 
        QueryParameters? parameters = null, 
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting visits scheduled before {Date}", date);
        
        // For "before" a date, end at the end of that day (inclusive)
        var endDateTime = date.Date.AddDays(1).AddSeconds(-1); // End of the given day at 23:59:59
        
        // Build filter for visits before a specific date
        var filter = new[]
        {
            new Dictionary<string, object>
            {
                ["property"] = "start_date",
                ["operator"] = "between",
                ["value"] = new[] { 
                    "1900-01-01 00:00:00",
                    endDateTime.ToString("yyyy-MM-dd HH:mm:ss")
                }
            }
        };
        
        var filterJson = System.Text.Json.JsonSerializer.Serialize(filter);
        var encodedFilter = HttpUtility.UrlEncode(filterJson);
        
        var start = parameters?.Start ?? 0;
        var limit = parameters?.Limit ?? 20;
        var endpoint = $"{VisitsEndpoint}?start={start}&limit={limit}&filters={encodedFilter}";
        
        var response = await _apiService.GetAsync<VisitsResponse>(endpoint, cancellationToken);
        return ConvertToPagedResponse(response, start, limit);
    }
    
    private string BuildQueryString(QueryParameters? parameters)
    {
        if (parameters == null)
            return string.Empty;
            
        var queryDict = parameters.ToDictionary();
        return string.Join("&", queryDict.Select(kvp => $"{kvp.Key}={Uri.EscapeDataString(kvp.Value)}"));
    }
    
    private string BuildVisitQueryString(VisitQueryParameters parameters)
    {
        var queryDict = parameters.ToDictionary();
        return string.Join("&", queryDict.Select(kvp => $"{kvp.Key}={Uri.EscapeDataString(kvp.Value)}"));
    }
    
    private PagedResponse<Visit> ConvertToPagedResponse(VisitsResponse response, int start, int limit)
    {
        var pagedResponse = new PagedResponse<Visit>
        {
            Success = true,
            Data = response.Visits?.Select(v => new Visit
            {
                Id = v.Id,
                WorkOrderId = v.WorkorderId,
                AssignmentId = v.AssignmentId,
                FacilityId = v.FacilityId,
                // Status is nested in object_state.status.name
                Status = v.ObjectState?.Status?.Name ?? string.Empty,
                Description = v.Category?.Description ?? v.Summary ?? string.Empty,
                StoreName = v.Store?.Name,
                Notes = v.WorkPerformed ?? v.Scope,
                WorkPerformed = v.WorkPerformed,
                Scope = v.Scope,
                CreatedAt = v.CreatedAt,
                UpdatedAt = v.UpdatedAt,
                // Important date fields
                StartDate = v.StartDate,
                EndDate = v.EndDate,
                CheckInTime = v.CheckInTime,
                CheckOutTime = v.CheckOutTime,
                // Map StartDate to ScheduledDate for backwards compatibility
                ScheduledDate = v.StartDate,
                ActualDate = v.CheckInTime != null ? v.CheckInTime.Value.Date : (DateTime?)null
            }).ToList() ?? new List<Visit>(),
            TotalCount = response.Visits?.Count ?? 0,
            Page = start / limit + 1,
            PageSize = limit
        };
        
        pagedResponse.TotalPages = (int)Math.Ceiling(pagedResponse.TotalCount / (double)pagedResponse.PageSize);
        
        return pagedResponse;
    }
    
    public async Task<List<Visit>> GetAllVisitsAsync(
        QueryParameters? baseParameters = null, 
        int maxPages = 10,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching all visits (up to {MaxPages} pages)", maxPages);
        
        var allVisits = new List<Visit>();
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
            
            var response = await GetVisitsAsync(parameters, cancellationToken);
            
            if (response.Data != null && response.Data.Any())
            {
                allVisits.AddRange(response.Data);
                _logger.LogDebug("Fetched page {Page} with {Count} visits. Total so far: {Total}", 
                    currentPage + 1, response.Data.Count(), allVisits.Count);
            }
            
            // Check if there are more pages
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
            
            var response = await GetVisitsByWorkOrderAsync(workOrderId, parameters, cancellationToken);
            
            if (response.Data != null && response.Data.Any())
            {
                allVisits.AddRange(response.Data);
                _logger.LogDebug("Fetched page {Page} with {Count} visits. Total so far: {Total}", 
                    currentPage + 1, response.Data.Count(), allVisits.Count);
            }
            
            // Check if there are more pages
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
            
            var response = await GetVisitsByDateRangeAsync(startDate, endDate, parameters, cancellationToken);
            
            if (response.Data != null && response.Data.Any())
            {
                allVisits.AddRange(response.Data);
                _logger.LogDebug("Fetched page {Page} with {Count} visits. Total so far: {Total}", 
                    currentPage + 1, response.Data.Count(), allVisits.Count);
            }
            
            // Check if there are more pages
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