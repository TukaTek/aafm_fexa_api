using Microsoft.AspNetCore.Mvc;
using Fexa.ApiClient.Services;
using Fexa.ApiClient.Models;

namespace Fexa.ApiClient.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VisitController : ControllerBase
{
    private readonly IVisitService _visitService;
    private readonly ILogger<VisitController> _logger;

    public VisitController(
        IVisitService visitService,
        ILogger<VisitController> logger)
    {
        _visitService = visitService;
        _logger = logger;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Visit>> GetVisit(int id)
    {
        try
        {
            _logger.LogInformation("Getting visit {Id}", id);
            var visit = await _visitService.GetVisitAsync(id);
            if (visit == null)
            {
                return NotFound();
            }
            return Ok(visit);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting visit {Id}", id);
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpGet("workorder/{workOrderId}")]
    public async Task<ActionResult<PagedResponse<Visit>>> GetVisitsByWorkOrder(int workOrderId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        try
        {
            _logger.LogInformation("Getting visits for work order {WorkOrderId}", workOrderId);
            var parameters = new QueryParameters { Page = page, PageSize = pageSize };
            var visits = await _visitService.GetVisitsByWorkOrderAsync(workOrderId, parameters);
            return Ok(visits);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting visits for work order {WorkOrderId}", workOrderId);
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpGet("daterange")]
    public async Task<ActionResult<PagedResponse<Visit>>> GetVisitsByDateRange(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        try
        {
            _logger.LogInformation("Getting visits between {StartDate} and {EndDate}", startDate, endDate);
            var parameters = new QueryParameters { Page = page, PageSize = pageSize };
            var visits = await _visitService.GetVisitsByDateRangeAsync(startDate, endDate, parameters);
            return Ok(visits);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting visits by date range");
            return StatusCode(500, new { error = ex.Message });
        }
    }
}