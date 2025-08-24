using Microsoft.Extensions.Logging;
using Fexa.ApiClient.Models;

namespace Fexa.ApiClient.Services;

public class SeverityService : ISeverityService
{
    private readonly IFexaApiService _apiService;
    private readonly ILogger<SeverityService> _logger;
    private const string BaseEndpoint = "/api/ev1/severities";

    public SeverityService(IFexaApiService apiService, ILogger<SeverityService> logger)
    {
        _apiService = apiService ?? throw new ArgumentNullException(nameof(apiService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<PagedResponse<Severity>> GetSeveritiesAsync(QueryParameters? parameters = null, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting severities with parameters: {Parameters}", parameters);
        
        var queryParams = parameters ?? new QueryParameters();
        var queryString = BuildQueryString(queryParams);
        var endpoint = $"{BaseEndpoint}{queryString}";
        
        var response = await _apiService.GetAsync<SeveritiesResponse>(endpoint, cancellationToken);
        
        return ConvertToPagedResponse(response, queryParams.Start, queryParams.Limit);
    }

    public async Task<Severity> GetSeverityAsync(int severityId, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting severity with ID: {SeverityId}", severityId);
        
        var endpoint = $"{BaseEndpoint}/{severityId}";
        var response = await _apiService.GetAsync<SingleSeverityResponse>(endpoint, cancellationToken);
        
        if (response?.Severity == null)
        {
            throw new InvalidOperationException($"Severity with ID {severityId} not found");
        }
        
        return response.Severity;
    }

    public async Task<PagedResponse<Severity>> GetActiveSeveritiesAsync(QueryParameters? parameters = null, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting active severities");
        
        var queryParams = parameters ?? new QueryParameters();
        queryParams.Filters = queryParams.Filters ?? new List<FexaFilter>();
        queryParams.Filters.Add(new FexaFilter("active", true));
        
        return await GetSeveritiesAsync(queryParams, cancellationToken);
    }

    public async Task<PagedResponse<Severity>> GetSeveritiesByLevelAsync(int level, QueryParameters? parameters = null, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting severities by level: {Level}", level);
        
        var queryParams = parameters ?? new QueryParameters();
        queryParams.Filters = queryParams.Filters ?? new List<FexaFilter>();
        queryParams.Filters.Add(new FexaFilter("level", level));
        
        return await GetSeveritiesAsync(queryParams, cancellationToken);
    }

    public async Task<List<Severity>> GetAllSeveritiesAsync(QueryParameters? baseParameters = null, int maxPages = 10, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching all severities (up to {MaxPages} pages)", maxPages);
        
        var allSeverities = new List<Severity>();
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
            
            var response = await GetSeveritiesAsync(parameters, cancellationToken);
            
            if (response.Data != null && response.Data.Any())
            {
                allSeverities.AddRange(response.Data);
                _logger.LogDebug("Fetched page {Page} with {Count} severities. Total so far: {Total}", 
                    currentPage + 1, response.Data.Count(), allSeverities.Count);
            }
            
            hasMoreData = response.Data != null && 
                         response.Data.Count() == pageSize && 
                         (response.TotalCount == 0 || allSeverities.Count < response.TotalCount);
            
            currentPage++;
        }
        
        _logger.LogInformation("Fetched {Total} severities across {Pages} pages", 
            allSeverities.Count, currentPage);
        
        return allSeverities;
    }

    public async Task<List<Severity>> GetAllActiveSeveritiesAsync(QueryParameters? baseParameters = null, int maxPages = 10, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching all active severities (up to {MaxPages} pages)", maxPages);
        
        var allSeverities = new List<Severity>();
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
            
            var response = await GetActiveSeveritiesAsync(parameters, cancellationToken);
            
            if (response.Data != null && response.Data.Any())
            {
                allSeverities.AddRange(response.Data);
                _logger.LogDebug("Fetched page {Page} with {Count} active severities. Total so far: {Total}", 
                    currentPage + 1, response.Data.Count(), allSeverities.Count);
            }
            
            hasMoreData = response.Data != null && 
                         response.Data.Count() == pageSize && 
                         (response.TotalCount == 0 || allSeverities.Count < response.TotalCount);
            
            currentPage++;
        }
        
        _logger.LogInformation("Fetched {Total} active severities across {Pages} pages", 
            allSeverities.Count, currentPage);
        
        return allSeverities;
    }

    public async Task<Severity> CreateSeverityAsync(CreateSeverityRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating severity: {Name}", request.Name);
        
        var response = await _apiService.PostAsync<SingleSeverityResponse>(BaseEndpoint, request, cancellationToken);
        
        if (response?.Severity == null)
        {
            throw new InvalidOperationException("Failed to create severity");
        }
        
        return response.Severity;
    }

    public async Task<Severity> UpdateSeverityAsync(int severityId, UpdateSeverityRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating severity: {SeverityId}", severityId);
        
        var endpoint = $"{BaseEndpoint}/{severityId}";
        var response = await _apiService.PutAsync<SingleSeverityResponse>(endpoint, request, cancellationToken);
        
        if (response?.Severity == null)
        {
            throw new InvalidOperationException($"Failed to update severity {severityId}");
        }
        
        return response.Severity;
    }

    public async Task DeleteSeverityAsync(int severityId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Deleting severity: {SeverityId}", severityId);
        
        var endpoint = $"{BaseEndpoint}/{severityId}";
        await _apiService.DeleteAsync<object>(endpoint, cancellationToken);
    }

    private string BuildQueryString(QueryParameters parameters)
    {
        var queryDict = new Dictionary<string, string>
        {
            ["start"] = parameters.Start.ToString(),
            ["limit"] = parameters.Limit.ToString()
        };
        
        if (!string.IsNullOrWhiteSpace(parameters.SortBy))
            queryDict["sortBy"] = parameters.SortBy;
            
        if (parameters.SortDescending)
            queryDict["sortDesc"] = "true";
        
        if (parameters.Filters?.Any() == true)
        {
            var filterJson = FilterBuilder.Create()
                .AddFilters(parameters.Filters)
                .ToUrlEncoded();
            queryDict["filters"] = filterJson;
        }
        
        return string.Join("&", queryDict.Select(kvp => $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value)}"));
    }

    private PagedResponse<Severity> ConvertToPagedResponse(SeveritiesResponse response, int start, int limit)
    {
        var pagedResponse = new PagedResponse<Severity>
        {
            Success = true,
            Data = response.Severities,
            TotalCount = response.Severities?.Count ?? 0,
            Page = start / limit + 1,
            PageSize = limit
        };
        
        pagedResponse.TotalPages = (int)Math.Ceiling(pagedResponse.TotalCount / (double)pagedResponse.PageSize);
        
        return pagedResponse;
    }
}
