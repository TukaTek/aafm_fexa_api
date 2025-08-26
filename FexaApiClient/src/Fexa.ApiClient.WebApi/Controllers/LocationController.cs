using Microsoft.AspNetCore.Mvc;
using Fexa.ApiClient.Services;
using Fexa.ApiClient.Models;

namespace Fexa.ApiClient.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LocationController : ControllerBase
{
    private readonly ILocationService _locationService;
    private readonly ILogger<LocationController> _logger;

    public LocationController(
        ILocationService locationService,
        ILogger<LocationController> logger)
    {
        _locationService = locationService;
        _logger = logger;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<LocationDto>> GetLocation(int id)
    {
        try
        {
            _logger.LogInformation("Getting location {LocationId}", id);
            var location = await _locationService.GetLocationAsync(id);
            return Ok(location);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Location {LocationId} not found", id);
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting location {LocationId}", id);
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpGet("client/{clientId}")]
    public async Task<ActionResult<List<LocationDto>>> GetLocationsByClient(int clientId)
    {
        try
        {
            _logger.LogInformation("Getting locations for client {ClientId}", clientId);
            var locations = await _locationService.GetLocationsByClientAsync(clientId);
            return Ok(locations);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting locations for client {ClientId}", clientId);
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpGet]
    public async Task<ActionResult<List<LocationDto>>> GetAllLocations()
    {
        try
        {
            _logger.LogInformation("Getting all locations");
            var locations = await _locationService.GetAllLocationsAsync();
            return Ok(locations);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all locations");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpGet("active")]
    public async Task<ActionResult<List<LocationDto>>> GetActiveLocations()
    {
        try
        {
            _logger.LogInformation("Getting active locations");
            var locations = await _locationService.GetActiveLocationsAsync();
            return Ok(locations);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active locations");
            return StatusCode(500, new { error = ex.Message });
        }
    }
}