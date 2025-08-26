using Microsoft.AspNetCore.Mvc;
using Fexa.ApiClient.Services;
using Fexa.ApiClient.Models;

namespace Fexa.ApiClient.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WorkOrderClassController : ControllerBase
{
    private readonly IWorkOrderClassService _workOrderClassService;
    private readonly ILogger<WorkOrderClassController> _logger;

    public WorkOrderClassController(
        IWorkOrderClassService workOrderClassService,
        ILogger<WorkOrderClassController> logger)
    {
        _workOrderClassService = workOrderClassService;
        _logger = logger;
    }

    /// <summary>
    /// Get all active work order classes
    /// </summary>
    /// <returns>List of active work order classes</returns>
    [HttpGet("active")]
    [ProducesResponseType(typeof(List<WorkOrderClassDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<List<WorkOrderClassDto>>> GetActiveWorkOrderClasses()
    {
        try
        {
            _logger.LogInformation("Getting active work order classes");
            
            var activeClasses = await _workOrderClassService.GetActiveWorkOrderClassesAsync();
            
            // Map to DTOs
            var dtos = activeClasses.Select(c => new WorkOrderClassDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                Active = c.Active,
                Default = c.Default,
                PmType = c.PmType
            }).ToList();
            
            _logger.LogInformation("Returning {Count} active work order classes", dtos.Count);
            return Ok(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active work order classes");
            return StatusCode(500, new { error = "An error occurred while retrieving work order classes" });
        }
    }

    /// <summary>
    /// Get all work order classes (including inactive)
    /// </summary>
    /// <returns>List of all work order classes</returns>
    [HttpGet]
    [ProducesResponseType(typeof(List<WorkOrderClassDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<List<WorkOrderClassDto>>> GetAllWorkOrderClasses()
    {
        try
        {
            _logger.LogInformation("Getting all work order classes");
            
            var allClasses = await _workOrderClassService.GetAllWorkOrderClassesAsync();
            
            // Map to DTOs
            var dtos = allClasses.Select(c => new WorkOrderClassDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                Active = c.Active,
                Default = c.Default,
                PmType = c.PmType
            }).ToList();
            
            _logger.LogInformation("Returning {Count} work order classes", dtos.Count);
            return Ok(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all work order classes");
            return StatusCode(500, new { error = "An error occurred while retrieving work order classes" });
        }
    }

    /// <summary>
    /// Get work order class by ID
    /// </summary>
    /// <param name="id">The work order class ID</param>
    /// <returns>Work order class details</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(WorkOrderClassDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<WorkOrderClassDto>> GetWorkOrderClassById(int id)
    {
        try
        {
            _logger.LogInformation("Getting work order class {Id}", id);
            
            var workOrderClass = await _workOrderClassService.GetWorkOrderClassByIdAsync(id);
            
            if (workOrderClass == null)
            {
                _logger.LogWarning("Work order class {Id} not found", id);
                return NotFound();
            }
            
            var dto = new WorkOrderClassDto
            {
                Id = workOrderClass.Id,
                Name = workOrderClass.Name,
                Description = workOrderClass.Description,
                Active = workOrderClass.Active,
                Default = workOrderClass.Default,
                PmType = workOrderClass.PmType
            };
            
            return Ok(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting work order class {Id}", id);
            return StatusCode(500, new { error = "An error occurred while retrieving the work order class" });
        }
    }

    /// <summary>
    /// Get work order class by name
    /// </summary>
    /// <param name="name">The work order class name</param>
    /// <returns>Work order class details</returns>
    [HttpGet("byname/{name}")]
    [ProducesResponseType(typeof(WorkOrderClassDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<WorkOrderClassDto>> GetWorkOrderClassByName(string name)
    {
        try
        {
            _logger.LogInformation("Getting work order class by name: {Name}", name);
            
            var workOrderClass = await _workOrderClassService.GetWorkOrderClassByNameAsync(name);
            
            if (workOrderClass == null)
            {
                _logger.LogWarning("Work order class '{Name}' not found", name);
                return NotFound();
            }
            
            var dto = new WorkOrderClassDto
            {
                Id = workOrderClass.Id,
                Name = workOrderClass.Name,
                Description = workOrderClass.Description,
                Active = workOrderClass.Active,
                Default = workOrderClass.Default,
                PmType = workOrderClass.PmType
            };
            
            return Ok(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting work order class by name: {Name}", name);
            return StatusCode(500, new { error = "An error occurred while retrieving the work order class" });
        }
    }

    /// <summary>
    /// Refresh work order classes cache
    /// </summary>
    /// <returns>Updated list of all work order classes</returns>
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(List<WorkOrderClassDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<List<WorkOrderClassDto>>> RefreshWorkOrderClasses()
    {
        try
        {
            _logger.LogInformation("Refreshing work order classes cache");
            
            var allClasses = await _workOrderClassService.RefreshWorkOrderClassesAsync();
            
            // Map to DTOs
            var dtos = allClasses.Select(c => new WorkOrderClassDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                Active = c.Active,
                Default = c.Default,
                PmType = c.PmType
            }).ToList();
            
            _logger.LogInformation("Cache refreshed with {Count} work order classes", dtos.Count);
            return Ok(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing work order classes cache");
            return StatusCode(500, new { error = "An error occurred while refreshing work order classes" });
        }
    }
}