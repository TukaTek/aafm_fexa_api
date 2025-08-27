using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Fexa.ApiClient.Services;
using Fexa.ApiClient.Models;
using Fexa.ApiClient.Configuration;
using Fexa.ApiClient.WebApi.Models;

namespace Fexa.ApiClient.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WorkOrderController : ControllerBase
{
    private readonly IWorkOrderService _workOrderService;
    private readonly ILogger<WorkOrderController> _logger;
    private readonly FexaApiOptions _options;

    public WorkOrderController(
        IWorkOrderService workOrderService,
        ILogger<WorkOrderController> logger,
        IOptions<FexaApiOptions> options)
    {
        _workOrderService = workOrderService;
        _logger = logger;
        _options = options.Value;
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
    
    [HttpPost]
    public async Task<ActionResult<WorkOrder>> CreateWorkOrder([FromBody] CreateWorkOrderRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            _logger.LogInformation("Creating work order for client {ClientId} at facility {FacilityId}", 
                request?.Workorders?.PlacedFor, request?.Workorders?.FacilityId);
            
            var workOrder = await _workOrderService.CreateWorkOrderAsync(request!);
            
            _logger.LogInformation("Successfully created work order {WorkOrderId}", workOrder.Id);
            return CreatedAtAction(nameof(GetWorkOrder), new { id = workOrder.Id }, workOrder);
        }
        catch (ArgumentNullException ex)
        {
            _logger.LogError(ex, "Null argument provided for work order creation");
            return BadRequest(new { error = ex.Message });
        }
        catch (ArgumentException ex)
        {
            _logger.LogError(ex, "Invalid argument provided for work order creation");
            return BadRequest(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Failed to create work order");
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error creating work order");
            return StatusCode(500, new { error = "An unexpected error occurred while creating the work order." });
        }
    }
    
    /// <summary>
    /// Create a work order with simplified field names
    /// </summary>
    /// <param name="dto">Simplified work order creation request</param>
    /// <returns>Created work order with simplified response</returns>
    [HttpPost("simple")]
    public async Task<ActionResult<CreateWorkOrderResponseDto>> CreateWorkOrderSimple([FromBody] CreateWorkOrderDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            _logger.LogInformation("Creating work order (simplified) for client {ClientId} at location {LocationId}", 
                dto.ClientId, dto.LocationId);
            
            // Map simplified DTO to Fexa format
            var request = new CreateWorkOrderRequest
            {
                Workorders = new WorkOrderData
                {
                    WorkorderClassId = dto.WorkOrderClassId ?? 1, // Default to 1 if not provided
                    PriorityId = dto.PriorityId,
                    CategoryId = dto.CategoryId,
                    Description = dto.Description,
                    FacilityId = dto.LocationId, // Map location_id to facility_id
                    PlacedBy = dto.CreatedByUserId ?? _options.DefaultUserId ?? 294608, // Use provided, default, or fallback
                    PlacedFor = dto.ClientId, // Map client_id to placed_for
                    ClientPurchaseOrderNumbers = string.IsNullOrEmpty(dto.ClientPoNumber) 
                        ? new List<ClientPurchaseOrder>()
                        : new List<ClientPurchaseOrder> 
                        { 
                            new ClientPurchaseOrder 
                            { 
                                PurchaseOrderNumber = dto.ClientPoNumber, 
                                Active = true 
                            } 
                        },
                    Notes = dto.Notes,
                    ScheduledFor = dto.ScheduledFor,
                    VendorId = dto.VendorId,
                    WorkOrderNumber = dto.WorkOrderNumber,
                    SeverityId = dto.SeverityId,
                    Emergency = dto.IsEmergency,
                    Ntn = dto.Ntn,
                    ClientNotes = dto.ClientNotes
                }
            };
            
            var workOrder = await _workOrderService.CreateWorkOrderAsync(request);
            
            // Map response to simplified DTO
            var response = new CreateWorkOrderResponseDto
            {
                Id = workOrder.Id,
                WorkOrderNumber = workOrder.WorkOrderNumber,
                Status = workOrder.Status,
                Description = workOrder.Description ?? dto.Description,
                ClientId = dto.ClientId,
                LocationId = dto.LocationId,
                CategoryId = dto.CategoryId,
                PriorityId = dto.PriorityId,
                CreatedAt = workOrder.CreatedAt ?? DateTime.UtcNow,
                ClientPoNumber = dto.ClientPoNumber,
                VendorId = workOrder.AssignedTo
            };
            
            _logger.LogInformation("Successfully created work order {WorkOrderId} (simplified)", workOrder.Id);
            return CreatedAtAction(nameof(GetWorkOrder), new { id = workOrder.Id }, response);
        }
        catch (ArgumentNullException ex)
        {
            _logger.LogError(ex, "Null argument provided for work order creation (simplified)");
            return BadRequest(new { error = ex.Message });
        }
        catch (ArgumentException ex)
        {
            _logger.LogError(ex, "Invalid argument provided for work order creation (simplified)");
            return BadRequest(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Failed to create work order (simplified)");
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error creating work order (simplified)");
            return StatusCode(500, new { error = "An unexpected error occurred while creating the work order." });
        }
    }
}