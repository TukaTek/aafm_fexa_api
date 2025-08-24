using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System.Net;
using Fexa.ApiClient.Services;
using Microsoft.Extensions.Configuration;

namespace Fexa.ApiClient.Function.Functions;

public class HealthFunctions
{
    private readonly ITokenService _tokenService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<HealthFunctions> _logger;

    public HealthFunctions(
        ITokenService tokenService,
        IConfiguration configuration,
        ILogger<HealthFunctions> logger)
    {
        _tokenService = tokenService;
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>
    /// Basic health check endpoint
    /// </summary>
    [Function("Health")]
    [OpenApiOperation(operationId: "Health", tags: new[] { "Health" }, Summary = "Health check", Description = "Returns the health status of the API.")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(HealthResponse), Description = "Service is healthy")]
    public async Task<HttpResponseData> Health(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "health")] 
        HttpRequestData req)
    {
        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(new 
        { 
            status = "healthy",
            service = "Fexa API Middleware",
            timestamp = DateTime.UtcNow
        });
        return response;
    }

    /// <summary>
    /// Detailed health check with connectivity status
    /// </summary>
    [Function("HealthDetailed")]
    [OpenApiOperation(operationId: "HealthDetailed", tags: new[] { "Health" }, Summary = "Detailed health check", Description = "Returns detailed health status including Fexa API connectivity.")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(DetailedHealthResponse), Description = "Detailed health status")]
    public async Task<HttpResponseData> HealthDetailed(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "health/detailed")] 
        HttpRequestData req)
    {
        try
        {
            _logger.LogInformation("Performing detailed health check");
            
            var fexaApiConfigured = !string.IsNullOrEmpty(_configuration["FexaApi:ClientId"]) &&
                                   !string.IsNullOrEmpty(_configuration["FexaApi:ClientSecret"]);
            
            bool fexaApiConnected = false;
            string? fexaApiError = null;
            
            if (fexaApiConfigured)
            {
                try
                {
                    var token = await _tokenService.GetAccessTokenAsync();
                    fexaApiConnected = !string.IsNullOrEmpty(token);
                }
                catch (Exception ex)
                {
                    fexaApiError = ex.Message;
                }
            }
            
            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(new 
            { 
                status = fexaApiConnected ? "healthy" : (fexaApiConfigured ? "degraded" : "unconfigured"),
                service = "Fexa API Middleware",
                timestamp = DateTime.UtcNow,
                details = new
                {
                    fexaApi = new
                    {
                        configured = fexaApiConfigured,
                        connected = fexaApiConnected,
                        baseUrl = _configuration["FexaApi:BaseUrl"],
                        error = fexaApiError
                    }
                }
            });
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during health check");
            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(new 
            { 
                status = "error",
                service = "Fexa API Middleware",
                timestamp = DateTime.UtcNow,
                error = ex.Message
            });
            return response;
        }
    }
}

public class HealthResponse
{
    public string Status { get; set; } = string.Empty;
    public string Service { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}

public class DetailedHealthResponse : HealthResponse
{
    public HealthDetails? Details { get; set; }
    public string? Error { get; set; }
}

public class HealthDetails
{
    public FexaApiHealth FexaApi { get; set; } = new();
}

public class FexaApiHealth
{
    public bool Configured { get; set; }
    public bool Connected { get; set; }
    public string? BaseUrl { get; set; }
    public string? Error { get; set; }
}