using Microsoft.AspNetCore.Mvc;
using Fexa.ApiClient.Services;
using Fexa.ApiClient.Models;

namespace Fexa.ApiClient.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TransitionController : ControllerBase
{
    private readonly ITransitionService _transitionService;
    private readonly IWorkOrderService _workOrderService;
    private readonly ILogger<TransitionController> _logger;

    public TransitionController(
        ITransitionService transitionService,
        IWorkOrderService workOrderService,
        ILogger<TransitionController> logger)
    {
        _transitionService = transitionService;
        _workOrderService = workOrderService;
        _logger = logger;
    }

    [HttpGet("workorder/{workOrderId}")]
    public async Task<ActionResult<List<WorkflowTransition>>> GetWorkOrderTransitions(int workOrderId)
    {
        try
        {
            _logger.LogInformation("Getting transitions for work order {WorkOrderId}", workOrderId);
            
            // Get the work order to find current status
            var workOrder = await _workOrderService.GetWorkOrderAsync(workOrderId);
            if (workOrder == null)
            {
                return NotFound(new { error = "Work order not found" });
            }

            // Get all transitions for Work Order type
            var allTransitions = await _transitionService.GetTransitionsByTypeAsync("Work Order");
            
            // Filter for transitions from current status
            var currentStatusId = workOrder.ObjectState?.Status?.Id;
            if (currentStatusId.HasValue)
            {
                var availableTransitions = allTransitions
                    .Where(t => t.FromStatus?.Id == currentStatusId.Value)
                    .ToList();
                    
                _logger.LogInformation("Found {Count} transitions from status {StatusId}", 
                    availableTransitions.Count, currentStatusId.Value);
                    
                return Ok(availableTransitions);
            }
            
            return Ok(new List<WorkflowTransition>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting transitions for work order {WorkOrderId}", workOrderId);
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpPut("workorder/{workOrderId}/status/{statusId}")]
    public async Task<ActionResult<WorkOrder>> UpdateWorkOrderStatus(int workOrderId, int statusId, [FromBody] StatusUpdateRequest? request = null)
    {
        try
        {
            _logger.LogInformation("Updating work order {WorkOrderId} to status {StatusId}", workOrderId, statusId);
            var result = await _workOrderService.UpdateStatusAsync(workOrderId, statusId, request?.Reason);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating work order {WorkOrderId} status", workOrderId);
            return StatusCode(500, new { error = ex.Message });
        }
    }
}

public class StatusUpdateRequest
{
    public string? Reason { get; set; }
}