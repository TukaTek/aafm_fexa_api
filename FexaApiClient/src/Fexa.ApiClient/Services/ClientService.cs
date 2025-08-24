using Microsoft.Extensions.Logging;
using Fexa.ApiClient.Models;
using System.Text.Json;
using System.Web;

namespace Fexa.ApiClient.Services;

public class ClientService : IClientService
{
    private readonly IFexaApiService _apiService;
    private readonly ILogger<ClientService> _logger;
    private const string BaseEndpoint = "/api/ev1/clients";

    public ClientService(IFexaApiService apiService, ILogger<ClientService> logger)
    {
        _apiService = apiService ?? throw new ArgumentNullException(nameof(apiService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<PagedResponse<Client>> GetClientsAsync(QueryParameters? parameters = null, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching clients with parameters: Start={Start}, Limit={Limit}", 
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

        var response = await _apiService.GetAsync<ClientsResponse>(endpoint, cancellationToken);

        if (response?.Clients == null)
        {
            _logger.LogWarning("Received null or empty response from clients endpoint");
            return new PagedResponse<Client>
            {
                Data = new List<Client>(),
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
        
        return new PagedResponse<Client>
        {
            Data = response.Clients,
            TotalCount = response.Total,
            Page = currentPage,
            PageSize = limit,
            TotalPages = totalPages,
            Success = true
        };
    }

    public async Task<Client?> GetClientAsync(int id, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching client with ID: {ClientId}", id);

        var endpoint = $"{BaseEndpoint}/{id}";
        
        try
        {
            var response = await _apiService.GetAsync<ClientResponse>(endpoint, cancellationToken);
            
            if (response?.Client == null)
            {
                _logger.LogWarning("Client with ID {ClientId} not found or response was empty", id);
                return null;
            }

            return response.Client;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching client with ID {ClientId}", id);
            throw;
        }
    }

    public async Task<List<Client>> GetAllClientsAsync(QueryParameters? baseParameters = null, int maxPages = 10, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching all clients (up to {MaxPages} pages)", maxPages);
        
        var allClients = new List<Client>();
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
            
            var response = await GetClientsAsync(parameters, cancellationToken);
            
            if (response.Data != null && response.Data.Any())
            {
                allClients.AddRange(response.Data);
                _logger.LogDebug("Fetched page {Page} with {Count} clients. Total so far: {Total}", 
                    currentPage + 1, response.Data.Count(), allClients.Count);
            }
            
            hasMoreData = response.Data != null && 
                         response.Data.Count() == pageSize && 
                         (response.TotalCount == 0 || allClients.Count < response.TotalCount);
            
            currentPage++;
        }
        
        _logger.LogInformation("Fetched {Total} clients across {Pages} pages", 
            allClients.Count, currentPage);
        
        return allClients;
    }
}