using Microsoft.Extensions.Logging;
using Fexa.ApiClient.Models;

namespace Fexa.ApiClient.Services;

public class RegionService : IRegionService
{
    private readonly IFexaApiService _apiService;
    private readonly ILogger<RegionService> _logger;
    private const string BaseEndpoint = "/api/ev1/regions";

    public RegionService(IFexaApiService apiService, ILogger<RegionService> logger)
    {
        _apiService = apiService ?? throw new ArgumentNullException(nameof(apiService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<PagedResponse<Region>> GetRegionsAsync(QueryParameters? parameters = null, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting regions with parameters: {Parameters}", parameters);
        
        var queryParams = parameters ?? new QueryParameters();
        var queryString = BuildQueryString(queryParams);
        var endpoint = $"{BaseEndpoint}{queryString}";
        
        var response = await _apiService.GetAsync<RegionsResponse>(endpoint, cancellationToken);
        
        return ConvertToPagedResponse(response, queryParams.Start, queryParams.Limit);
    }

    public async Task<Region> GetRegionAsync(int regionId, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting region with ID: {RegionId}", regionId);
        
        var endpoint = $"{BaseEndpoint}/{regionId}";
        var response = await _apiService.GetAsync<SingleRegionResponse>(endpoint, cancellationToken);
        
        if (response?.Region == null)
        {
            throw new InvalidOperationException($"Region with ID {regionId} not found");
        }
        
        return response.Region;
    }

    public async Task<PagedResponse<Region>> GetRegionsByParentAsync(int parentId, QueryParameters? parameters = null, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting regions by parent: {ParentId}", parentId);
        
        var queryParams = parameters ?? new QueryParameters();
        queryParams.Filters = queryParams.Filters ?? new List<FexaFilter>();
        queryParams.Filters.Add(new FexaFilter("parent_id", parentId));
        
        return await GetRegionsAsync(queryParams, cancellationToken);
    }

    public async Task<PagedResponse<Region>> GetActiveRegionsAsync(QueryParameters? parameters = null, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting active regions");
        
        var queryParams = parameters ?? new QueryParameters();
        queryParams.Filters = queryParams.Filters ?? new List<FexaFilter>();
        queryParams.Filters.Add(new FexaFilter("active", true));
        
        return await GetRegionsAsync(queryParams, cancellationToken);
    }

    public async Task<PagedResponse<Region>> GetRegionsByLevelAsync(int level, QueryParameters? parameters = null, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting regions by level: {Level}", level);
        
        var queryParams = parameters ?? new QueryParameters();
        queryParams.Filters = queryParams.Filters ?? new List<FexaFilter>();
        queryParams.Filters.Add(new FexaFilter("level", level));
        
        return await GetRegionsAsync(queryParams, cancellationToken);
    }

    public async Task<List<Region>> GetAllRegionsAsync(QueryParameters? baseParameters = null, int maxPages = 10, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching all regions (up to {MaxPages} pages)", maxPages);
        
        var allRegions = new List<Region>();
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
            
            var response = await GetRegionsAsync(parameters, cancellationToken);
            
            if (response.Data != null && response.Data.Any())
            {
                allRegions.AddRange(response.Data);
                _logger.LogDebug("Fetched page {Page} with {Count} regions. Total so far: {Total}", 
                    currentPage + 1, response.Data.Count(), allRegions.Count);
            }
            
            hasMoreData = response.Data != null && 
                         response.Data.Count() == pageSize && 
                         (response.TotalCount == 0 || allRegions.Count < response.TotalCount);
            
            currentPage++;
        }
        
        _logger.LogInformation("Fetched {Total} regions across {Pages} pages", 
            allRegions.Count, currentPage);
        
        return allRegions;
    }

    public async Task<List<Region>> GetAllActiveRegionsAsync(QueryParameters? baseParameters = null, int maxPages = 10, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching all active regions (up to {MaxPages} pages)", maxPages);
        
        var allRegions = new List<Region>();
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
            
            var response = await GetActiveRegionsAsync(parameters, cancellationToken);
            
            if (response.Data != null && response.Data.Any())
            {
                allRegions.AddRange(response.Data);
                _logger.LogDebug("Fetched page {Page} with {Count} active regions. Total so far: {Total}", 
                    currentPage + 1, response.Data.Count(), allRegions.Count);
            }
            
            hasMoreData = response.Data != null && 
                         response.Data.Count() == pageSize && 
                         (response.TotalCount == 0 || allRegions.Count < response.TotalCount);
            
            currentPage++;
        }
        
        _logger.LogInformation("Fetched {Total} active regions across {Pages} pages", 
            allRegions.Count, currentPage);
        
        return allRegions;
    }

    public async Task<Region> CreateRegionAsync(CreateRegionRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating region: {Name}", request.Name);
        
        var response = await _apiService.PostAsync<SingleRegionResponse>(BaseEndpoint, request, cancellationToken);
        
        if (response?.Region == null)
        {
            throw new InvalidOperationException("Failed to create region");
        }
        
        return response.Region;
    }

    public async Task<Region> UpdateRegionAsync(int regionId, UpdateRegionRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating region: {RegionId}", regionId);
        
        var endpoint = $"{BaseEndpoint}/{regionId}";
        var response = await _apiService.PutAsync<SingleRegionResponse>(endpoint, request, cancellationToken);
        
        if (response?.Region == null)
        {
            throw new InvalidOperationException($"Failed to update region {regionId}");
        }
        
        return response.Region;
    }

    public async Task DeleteRegionAsync(int regionId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Deleting region: {RegionId}", regionId);
        
        var endpoint = $"{BaseEndpoint}/{regionId}";
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

    private PagedResponse<Region> ConvertToPagedResponse(RegionsResponse response, int start, int limit)
    {
        var pagedResponse = new PagedResponse<Region>
        {
            Success = true,
            Data = response.Regions,
            TotalCount = response.Regions?.Count ?? 0,
            Page = start / limit + 1,
            PageSize = limit
        };
        
        pagedResponse.TotalPages = (int)Math.Ceiling(pagedResponse.TotalCount / (double)pagedResponse.PageSize);
        
        return pagedResponse;
    }
}
