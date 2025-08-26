using Microsoft.AspNetCore.Mvc;
using Fexa.ApiClient.Services;
using Fexa.ApiClient.Models;

namespace Fexa.ApiClient.WebApi.Controllers;

/// <summary>
/// Provides fast cached access to client ID and Name lookups
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class CachedClientController : ControllerBase
{
    private readonly ICachedClientService _cachedClientService;
    private readonly ILogger<CachedClientController> _logger;

    public CachedClientController(
        ICachedClientService cachedClientService,
        ILogger<CachedClientController> logger)
    {
        _cachedClientService = cachedClientService;
        _logger = logger;
    }

    /// <summary>
    /// Gets all cached client info (ID and Name only) - very fast
    /// </summary>
    [HttpGet("info")]
    [ProducesResponseType(typeof(CachedClientResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<CachedClientResponse>> GetAllClientInfo()
    {
        try
        {
            _logger.LogInformation("Getting all cached client info");
            var clients = await _cachedClientService.GetAllClientInfoAsync();
            var status = await _cachedClientService.GetCacheStatusAsync();
            
            return Ok(new CachedClientResponse
            {
                Clients = clients,
                TotalCached = clients.Count,
                CacheAge = status.CacheAge,
                LastRefreshed = status.LastRefreshed
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cached client info");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Gets only active clients from cache
    /// </summary>
    [HttpGet("info/active")]
    [ProducesResponseType(typeof(List<ClientInfo>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ClientInfo>>> GetActiveClientInfo()
    {
        try
        {
            _logger.LogInformation("Getting active cached client info");
            var clients = await _cachedClientService.GetActiveClientInfoAsync();
            return Ok(clients);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active cached client info");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Gets a specific client by ID from cache
    /// </summary>
    [HttpGet("info/{id}")]
    [ProducesResponseType(typeof(ClientInfo), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ClientInfo>> GetClientInfoById(int id)
    {
        try
        {
            _logger.LogInformation("Getting cached client info for ID {Id}", id);
            var client = await _cachedClientService.GetClientInfoByIdAsync(id);
            
            if (client == null)
            {
                return NotFound(new { error = $"Client {id} not found in cache" });
            }
            
            return Ok(client);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cached client info for ID {Id}", id);
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Gets a client by name (exact match)
    /// </summary>
    [HttpGet("info/byname/{name}")]
    [ProducesResponseType(typeof(ClientInfo), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ClientInfo>> GetClientInfoByName(string name)
    {
        try
        {
            _logger.LogInformation("Getting cached client info for name '{Name}'", name);
            var client = await _cachedClientService.GetClientInfoByNameAsync(name);
            
            if (client == null)
            {
                return NotFound(new { error = $"Client '{name}' not found in cache" });
            }
            
            return Ok(client);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cached client info for name '{Name}'", name);
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Gets a client by IVR ID
    /// </summary>
    [HttpGet("info/byivr/{ivrId}")]
    [ProducesResponseType(typeof(ClientInfo), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ClientInfo>> GetClientInfoByIvrId(string ivrId)
    {
        try
        {
            _logger.LogInformation("Getting cached client info for IVR ID '{IvrId}'", ivrId);
            var client = await _cachedClientService.GetClientInfoByIvrIdAsync(ivrId);
            
            if (client == null)
            {
                return NotFound(new { error = $"Client with IVR ID '{ivrId}' not found in cache" });
            }
            
            return Ok(client);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cached client info for IVR ID '{IvrId}'", ivrId);
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Search clients by partial name match
    /// </summary>
    [HttpGet("search")]
    [ProducesResponseType(typeof(List<ClientInfo>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ClientInfo>>> SearchClientInfo([FromQuery] string q)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(q))
            {
                return BadRequest(new { error = "Search query 'q' is required" });
            }
            
            _logger.LogInformation("Searching cached clients for '{Query}'", q);
            var clients = await _cachedClientService.SearchClientInfoAsync(q);
            return Ok(clients);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching cached clients");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Gets the current cache status
    /// </summary>
    [HttpGet("cache-status")]
    [ProducesResponseType(typeof(ClientCacheStatus), StatusCodes.Status200OK)]
    public async Task<ActionResult<ClientCacheStatus>> GetCacheStatus()
    {
        try
        {
            var status = await _cachedClientService.GetCacheStatusAsync();
            return Ok(status);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cache status");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Forces a synchronous cache refresh (waits for completion)
    /// </summary>
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> RefreshCacheSync()
    {
        try
        {
            _logger.LogInformation("Starting synchronous client cache refresh");
            var clients = await _cachedClientService.RefreshCacheAsync();
            return Ok(new 
            { 
                message = "Client cache refreshed successfully",
                itemCount = clients.Count,
                activeCount = clients.Count(c => c.Active),
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing client cache");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Starts an asynchronous background cache refresh
    /// </summary>
    [HttpPost("refresh-async")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    public async Task<IActionResult> RefreshCacheAsync()
    {
        try
        {
            _logger.LogInformation("Starting asynchronous client cache refresh");
            await _cachedClientService.RefreshCacheInBackgroundAsync();
            return Accepted(new 
            { 
                message = "Client cache refresh started in background",
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting client cache refresh");
            return StatusCode(500, new { error = ex.Message });
        }
    }
}