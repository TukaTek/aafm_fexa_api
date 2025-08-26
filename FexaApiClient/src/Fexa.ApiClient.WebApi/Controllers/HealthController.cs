using Microsoft.AspNetCore.Mvc;
using Fexa.ApiClient.Services;

namespace Fexa.ApiClient.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly ITokenService _tokenService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<HealthController> _logger;

    public HealthController(
        ITokenService tokenService,
        IConfiguration configuration,
        ILogger<HealthController> logger)
    {
        _tokenService = tokenService;
        _configuration = configuration;
        _logger = logger;
    }

    [HttpGet]
    public IActionResult GetHealth()
    {
        return Ok(new
        {
            status = "healthy",
            service = "Fexa API Middleware",
            timestamp = DateTime.UtcNow
        });
    }

    [HttpGet("detailed")]
    public async Task<IActionResult> GetDetailedHealth()
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

            return Ok(new
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
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during health check");
            return Ok(new
            {
                status = "error",
                service = "Fexa API Middleware",
                timestamp = DateTime.UtcNow,
                error = ex.Message
            });
        }
    }
}