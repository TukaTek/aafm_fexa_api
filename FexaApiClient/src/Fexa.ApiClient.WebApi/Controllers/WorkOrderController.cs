using Microsoft.AspNetCore.Mvc;
using Fexa.ApiClient.Services;
using Fexa.ApiClient.Models;

namespace Fexa.ApiClient.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WorkOrderController : ControllerBase
{
    private readonly IWorkOrderService _workOrderService;
    private readonly ILogger<WorkOrderController> _logger;

    public WorkOrderController(
        IWorkOrderService workOrderService,
        ILogger<WorkOrderController> logger)
    {
        _workOrderService = workOrderService;
        _logger = logger;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<WorkOrder>> GetWorkOrder(int id)
    {
        try
        {
            _logger.LogInformation("Getting work order {Id}", id);
            var workOrder = await _workOrderService.GetWorkOrderAsync(id);
            return Ok(workOrder);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting work order {Id}", id);
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpGet("vendor/{vendorId}")]
    public async Task<ActionResult<PagedResponse<WorkOrder>>> GetWorkOrdersByVendor(
        int vendorId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        try
        {
            _logger.LogInformation("Getting work orders for vendor {VendorId}", vendorId);
            var parameters = new QueryParameters { Page = page, PageSize = pageSize };
            var workOrders = await _workOrderService.GetWorkOrdersByVendorAsync(vendorId, parameters);
            return Ok(workOrders);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting work orders for vendor {VendorId}", vendorId);
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpGet("client/{clientId}")]
    public async Task<ActionResult<PagedResponse<WorkOrder>>> GetWorkOrdersByClient(
        int clientId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        try
        {
            _logger.LogInformation("Getting work orders for client {ClientId}", clientId);
            var parameters = new QueryParameters { Page = page, PageSize = pageSize };
            var workOrders = await _workOrderService.GetWorkOrdersByClientAsync(clientId, parameters);
            return Ok(workOrders);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting work orders for client {ClientId}", clientId);
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpGet("clientpo/{poNumber}")]
    public async Task<ActionResult<List<WorkOrder>>> GetWorkOrdersByClientPO(
        string poNumber,
        [FromQuery] int maxPages = 10)
    {
        try
        {
            _logger.LogInformation("Getting work orders for Client PO {PONumber} (max pages: {MaxPages})", poNumber, maxPages);
            var workOrders = await _workOrderService.GetAllWorkOrdersByClientPOAsync(poNumber, null, maxPages);
            return Ok(workOrders);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting work orders for Client PO {PONumber}", poNumber);
            return StatusCode(500, new { error = ex.Message });
        }
    }
}