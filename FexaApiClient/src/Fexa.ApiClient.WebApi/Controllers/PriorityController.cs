using Microsoft.AspNetCore.Mvc;
using Fexa.ApiClient.Services;
using Fexa.ApiClient.Models;

namespace Fexa.ApiClient.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PriorityController : ControllerBase
{
    private readonly IPriorityService _priorityService;
    private readonly ILogger<PriorityController> _logger;

    public PriorityController(
        IPriorityService priorityService,
        ILogger<PriorityController> logger)
    {
        _priorityService = priorityService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<List<PriorityDto>>> GetAllPriorities()
    {
        try
        {
            _logger.LogInformation("Getting all priorities");
            var priorities = await _priorityService.GetAllPrioritiesAsync();
            var dtos = priorities.Select(p => new PriorityDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description
            }).ToList();
            return Ok(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all priorities");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpGet("active")]
    public async Task<ActionResult<List<PriorityDto>>> GetActivePriorities()
    {
        try
        {
            _logger.LogInformation("Getting active priorities");
            var priorities = await _priorityService.GetActivePrioritiesAsync();
            var dtos = priorities.Select(p => new PriorityDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description
            }).ToList();
            return Ok(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active priorities");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<PriorityDto>> GetPriority(int id)
    {
        try
        {
            _logger.LogInformation("Getting priority {Id}", id);
            var priority = await _priorityService.GetPriorityByIdAsync(id);
            if (priority == null)
            {
                return NotFound();
            }
            var dto = new PriorityDto
            {
                Id = priority.Id,
                Name = priority.Name,
                Description = priority.Description
            };
            return Ok(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting priority {Id}", id);
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpGet("byname/{name}")]
    public async Task<ActionResult<PriorityDto>> GetPriorityByName(string name)
    {
        try
        {
            _logger.LogInformation("Getting priority by name {Name}", name);
            var priority = await _priorityService.GetPriorityByNameAsync(name);
            if (priority == null)
            {
                return NotFound();
            }
            var dto = new PriorityDto
            {
                Id = priority.Id,
                Name = priority.Name,
                Description = priority.Description
            };
            return Ok(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting priority by name {Name}", name);
            return StatusCode(500, new { error = ex.Message });
        }
    }
}