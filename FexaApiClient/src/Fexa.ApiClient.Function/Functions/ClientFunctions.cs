using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System.Net;
using Fexa.ApiClient.Services;
using Fexa.ApiClient.Models;

namespace Fexa.ApiClient.Function.Functions;

public class ClientFunctions
{
    private readonly IClientService _clientService;
    private readonly ILogger<ClientFunctions> _logger;

    public ClientFunctions(IClientService clientService, ILogger<ClientFunctions> logger)
    {
        _clientService = clientService ?? throw new ArgumentNullException(nameof(clientService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets all clients
    /// </summary>
    /// <param name="req">HTTP request</param>
    /// <returns>List of clients</returns>
    [Function("GetClients")]
    [OpenApiOperation(operationId: "GetClients", tags: new[] { "Clients" }, Summary = "Get all clients", Description = "Retrieves a paginated list of all clients.")]
    [OpenApiParameter(name: "start", In = ParameterLocation.Query, Required = false, Type = typeof(int), Description = "Starting position for pagination")]
    [OpenApiParameter(name: "limit", In = ParameterLocation.Query, Required = false, Type = typeof(int), Description = "Number of records to return")]
    [OpenApiParameter(name: "sortBy", In = ParameterLocation.Query, Required = false, Type = typeof(string), Description = "Field to sort by")]
    [OpenApiParameter(name: "sortDesc", In = ParameterLocation.Query, Required = false, Type = typeof(bool), Description = "Sort in descending order")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(PagedResponse<Client>), Description = "List of clients")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(ErrorResponse), Description = "Internal server error")]
    public async Task<HttpResponseData> GetClients(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "clients")] 
        HttpRequestData req)
    {
        try
        {
            _logger.LogInformation("Getting all clients");
            
            var parameters = ParseQueryParameters(req);
            var clients = await _clientService.GetClientsAsync(parameters);
            
            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(clients);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting clients");
            var response = req.CreateResponse(HttpStatusCode.InternalServerError);
            await response.WriteAsJsonAsync(new ErrorResponse { Error = ex.Message });
            return response;
        }
    }

    /// <summary>
    /// Gets a specific client by ID
    /// </summary>
    /// <param name="req">HTTP request</param>
    /// <param name="id">Client ID</param>
    /// <returns>Client details</returns>
    [Function("GetClient")]
    [OpenApiOperation(operationId: "GetClient", tags: new[] { "Clients" }, Summary = "Get client by ID", Description = "Retrieves details of a specific client.")]
    [OpenApiParameter(name: "id", In = ParameterLocation.Path, Required = true, Type = typeof(int), Description = "The client ID")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(Client), Description = "Client details")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.NotFound, contentType: "application/json", bodyType: typeof(ErrorResponse), Description = "Client not found")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(ErrorResponse), Description = "Internal server error")]
    public async Task<HttpResponseData> GetClient(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "clients/{id}")] 
        HttpRequestData req,
        int id)
    {
        try
        {
            _logger.LogInformation("Getting client {ClientId}", id);
            
            var client = await _clientService.GetClientAsync(id);
            
            if (client == null)
            {
                var notFoundResponse = req.CreateResponse(HttpStatusCode.NotFound);
                await notFoundResponse.WriteAsJsonAsync(new ErrorResponse { Error = "Client not found" });
                return notFoundResponse;
            }
            
            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(client);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting client {ClientId}", id);
            var response = req.CreateResponse(HttpStatusCode.InternalServerError);
            await response.WriteAsJsonAsync(new ErrorResponse { Error = ex.Message });
            return response;
        }
    }

    /// <summary>
    /// Gets all clients (fetches all pages)
    /// </summary>
    /// <param name="req">HTTP request</param>
    /// <returns>All clients</returns>
    [Function("GetAllClients")]
    [OpenApiOperation(operationId: "GetAllClients", tags: new[] { "Clients" }, Summary = "Get all clients (all pages)", Description = "Retrieves all clients by fetching all pages.")]
    [OpenApiParameter(name: "maxPages", In = ParameterLocation.Query, Required = false, Type = typeof(int), Description = "Maximum number of pages to fetch (default: 10)")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(List<Client>), Description = "List of all clients")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(ErrorResponse), Description = "Internal server error")]
    public async Task<HttpResponseData> GetAllClients(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "clients/all")] 
        HttpRequestData req)
    {
        try
        {
            _logger.LogInformation("Getting all clients (all pages)");
            
            var maxPages = 10;
            if (req.Query["maxPages"] != null && int.TryParse(req.Query["maxPages"], out var mp))
            {
                maxPages = mp;
            }
            
            var clients = await _clientService.GetAllClientsAsync(maxPages: maxPages);
            
            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(new
            {
                count = clients.Count,
                clients = clients
            });
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all clients");
            var response = req.CreateResponse(HttpStatusCode.InternalServerError);
            await response.WriteAsJsonAsync(new ErrorResponse { Error = ex.Message });
            return response;
        }
    }

    private QueryParameters ParseQueryParameters(HttpRequestData req)
    {
        var query = System.Web.HttpUtility.ParseQueryString(req.Url.Query);
        
        var parameters = new QueryParameters
        {
            Start = int.TryParse(query["start"], out var start) ? start : 0,
            Limit = int.TryParse(query["limit"], out var limit) ? limit : 100,
            SortBy = query["sortBy"],
            SortDescending = bool.TryParse(query["sortDesc"], out var sortDesc) && sortDesc
        };
        
        return parameters;
    }
}

public class ErrorResponse
{
    public string Error { get; set; } = string.Empty;
}