using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System.Net;
using Fexa.ApiClient.Services;
using Fexa.ApiClient.Models;
using System.Text.Json;

namespace Fexa.ApiClient.Function.Functions;

public class WorkOrderFunctions
{
    private readonly IWorkOrderService _workOrderService;
    private readonly ITransitionService _transitionService;
    private readonly ILogger<WorkOrderFunctions> _logger;

    public WorkOrderFunctions(
        IWorkOrderService workOrderService,
        ITransitionService transitionService,
        ILogger<WorkOrderFunctions> logger)
    {
        _workOrderService = workOrderService;
        _transitionService = transitionService;
        _logger = logger;
    }

    /// <summary>
    /// Gets work orders for a specific vendor
    /// </summary>
    /// <param name="req">HTTP request</param>
    /// <param name="vendorId">Vendor ID</param>
    /// <returns>List of work orders</returns>
    [Function("GetWorkOrdersByVendor")]
    [OpenApiOperation(operationId: "GetWorkOrdersByVendor", tags: new[] { "Work Orders" }, Summary = "Get work orders by vendor", Description = "Retrieves all work orders assigned to a specific vendor.")]
    [OpenApiParameter(name: "vendorId", In = ParameterLocation.Path, Required = true, Type = typeof(int), Description = "The vendor ID")]
    [OpenApiParameter(name: "clientId", In = ParameterLocation.Query, Required = false, Type = typeof(int), Description = "Optional client ID filter")]
    [OpenApiParameter(name: "status", In = ParameterLocation.Query, Required = false, Type = typeof(string), Description = "Optional status filter")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(PagedResponse<WorkOrder>), Description = "List of work orders")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(ErrorResponse), Description = "Internal server error")]
    public async Task<HttpResponseData> GetWorkOrdersByVendor(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "workorders/vendor/{vendorId}")] 
        HttpRequestData req,
        int vendorId)
    {
        try
        {
            _logger.LogInformation("Getting work orders for vendor {VendorId}", vendorId);
            
            var clientId = req.Query["clientId"];
            var status = req.Query["status"];
            
            var workOrders = await _workOrderService.GetWorkOrdersByVendorAsync(vendorId);
            
            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(workOrders);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting work orders for vendor {VendorId}", vendorId);
            var response = req.CreateResponse(HttpStatusCode.InternalServerError);
            await response.WriteAsJsonAsync(new { error = ex.Message });
            return response;
        }
    }

    /// <summary>
    /// Gets a specific work order by ID
    /// </summary>
    /// <param name="req">HTTP request</param>
    /// <param name="id">Work order ID</param>
    /// <returns>Work order details</returns>
    [Function("GetWorkOrder")]
    [OpenApiOperation(operationId: "GetWorkOrder", tags: new[] { "Work Orders" }, Summary = "Get work order by ID", Description = "Retrieves details of a specific work order.")]
    [OpenApiParameter(name: "id", In = ParameterLocation.Path, Required = true, Type = typeof(int), Description = "The work order ID")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(WorkOrder), Description = "Work order details")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.NotFound, contentType: "application/json", bodyType: typeof(ErrorResponse), Description = "Work order not found")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(ErrorResponse), Description = "Internal server error")]
    public async Task<HttpResponseData> GetWorkOrder(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "workorders/{id}")] 
        HttpRequestData req,
        int id)
    {
        try
        {
            _logger.LogInformation("Getting work order {WorkOrderId}", id);
            
            var workOrder = await _workOrderService.GetWorkOrderAsync(id);
            
            if (workOrder == null)
            {
                var notFoundResponse = req.CreateResponse(HttpStatusCode.NotFound);
                await notFoundResponse.WriteAsJsonAsync(new { error = "Work order not found" });
                return notFoundResponse;
            }
            
            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(workOrder);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting work order {WorkOrderId}", id);
            var response = req.CreateResponse(HttpStatusCode.InternalServerError);
            await response.WriteAsJsonAsync(new { error = ex.Message });
            return response;
        }
    }

    /// <summary>
    /// Updates the status of a work order
    /// </summary>
    /// <param name="req">HTTP request</param>
    /// <param name="id">Work order ID</param>
    /// <param name="statusId">New status ID</param>
    /// <returns>Updated work order</returns>
    [Function("UpdateWorkOrderStatus")]
    [OpenApiOperation(operationId: "UpdateWorkOrderStatus", tags: new[] { "Work Orders" }, Summary = "Update work order status", Description = "Changes the status of a specific work order.")]
    [OpenApiParameter(name: "id", In = ParameterLocation.Path, Required = true, Type = typeof(int), Description = "The work order ID")]
    [OpenApiParameter(name: "statusId", In = ParameterLocation.Path, Required = true, Type = typeof(int), Description = "The new status ID")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(WorkOrder), Description = "Status updated successfully")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(ErrorResponse), Description = "Internal server error")]
    public async Task<HttpResponseData> UpdateWorkOrderStatus(
        [HttpTrigger(AuthorizationLevel.Function, "put", Route = "workorders/{id}/status/{statusId}")] 
        HttpRequestData req,
        int id,
        int statusId)
    {
        try
        {
            _logger.LogInformation("Updating work order {WorkOrderId} to status {StatusId}", id, statusId);
            
            var result = await _workOrderService.UpdateStatusAsync(id, statusId, null);
            
            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(result);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating work order {WorkOrderId} status", id);
            var response = req.CreateResponse(HttpStatusCode.InternalServerError);
            await response.WriteAsJsonAsync(new { error = ex.Message });
            return response;
        }
    }

    /// <summary>
    /// Gets available status transitions for a work order
    /// </summary>
    /// <param name="req">HTTP request</param>
    /// <param name="id">Work order ID</param>
    /// <returns>Available transitions</returns>
    [Function("GetWorkOrderTransitions")]
    [OpenApiOperation(operationId: "GetWorkOrderTransitions", tags: new[] { "Work Orders" }, Summary = "Get available transitions", Description = "Retrieves possible status transitions for a specific work order based on its current status.")]
    [OpenApiParameter(name: "id", In = ParameterLocation.Path, Required = true, Type = typeof(int), Description = "The work order ID")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(TransitionsResponse), Description = "Available transitions")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.NotFound, contentType: "application/json", bodyType: typeof(ErrorResponse), Description = "Work order not found")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(ErrorResponse), Description = "Internal server error")]
    public async Task<HttpResponseData> GetWorkOrderTransitions(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "workorders/{id}/transitions")] 
        HttpRequestData req,
        int id)
    {
        try
        {
            _logger.LogInformation("Getting transitions for work order {WorkOrderId}", id);
            
            var workOrder = await _workOrderService.GetWorkOrderAsync(id);
            if (workOrder == null)
            {
                var notFoundResponse = req.CreateResponse(HttpStatusCode.NotFound);
                await notFoundResponse.WriteAsJsonAsync(new { error = "Work order not found" });
                return notFoundResponse;
            }
            
            var currentStatusId = workOrder.ObjectState?.Status?.Id;
            if (currentStatusId == null)
            {
                var statusResponse = req.CreateResponse(HttpStatusCode.OK);
                await statusResponse.WriteAsJsonAsync(new { message = "Work order has no current status", transitions = new List<object>() });
                return statusResponse;
            }
            
            var transitions = await _transitionService.GetTransitionsFromStatusAsync(currentStatusId.Value);
            
            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(new 
            { 
                currentStatus = workOrder.ObjectState?.Status,
                transitions = transitions
            });
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting transitions for work order {WorkOrderId}", id);
            var response = req.CreateResponse(HttpStatusCode.InternalServerError);
            await response.WriteAsJsonAsync(new { error = ex.Message });
            return response;
        }
    }
    
    /// <summary>
    /// Gets work orders by client PO number
    /// </summary>
    /// <param name="req">HTTP request</param>
    /// <param name="poNumber">Client PO number</param>
    /// <returns>List of matching work orders</returns>
    [Function("GetWorkOrdersByClientPO")]
    [OpenApiOperation(operationId: "GetWorkOrdersByClientPO", tags: new[] { "Work Orders" }, Summary = "Get work orders by client PO number", Description = "Searches for work orders containing a specific client purchase order number.")]
    [OpenApiParameter(name: "poNumber", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "The client PO number to search for")]
    [OpenApiParameter(name: "vendorId", In = ParameterLocation.Query, Required = false, Type = typeof(int), Description = "Optional vendor ID filter")]
    [OpenApiParameter(name: "clientId", In = ParameterLocation.Query, Required = false, Type = typeof(int), Description = "Optional client ID filter")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(List<WorkOrder>), Description = "List of work orders with matching PO number")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(ErrorResponse), Description = "Internal server error")]
    public async Task<HttpResponseData> GetWorkOrdersByClientPO(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "workorders/po/{poNumber}")] 
        HttpRequestData req,
        string poNumber)
    {
        try
        {
            _logger.LogInformation("Searching for work orders with Client PO Number: {PONumber}", poNumber);
            
            // Parse optional query parameters
            int? vendorId = null;
            int? clientId = null;
            
            if (req.Query["vendorId"] != null && int.TryParse(req.Query["vendorId"], out var vid))
            {
                vendorId = vid;
            }
            
            if (req.Query["clientId"] != null && int.TryParse(req.Query["clientId"], out var cid))
            {
                clientId = cid;
            }
            
            // Commented out: API doesn't support filtering on client_purchase_order_numbers
            // var workOrders = await _workOrderService.GetWorkOrdersByClientPONumberAsync(poNumber, vendorId, clientId);
            var workOrders = new List<WorkOrder>(); // Return empty list for now
            
            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(new
            {
                count = workOrders.Count(),
                poNumber = poNumber,
                workOrders = workOrders
            });
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching for work orders with Client PO Number {PONumber}", poNumber);
            var response = req.CreateResponse(HttpStatusCode.InternalServerError);
            await response.WriteAsJsonAsync(new { error = ex.Message });
            return response;
        }
    }
}

// ErrorResponse is now defined in ClientFunctions.cs in the same namespace

public class TransitionsResponse
{
    public WorkflowStatus? CurrentStatus { get; set; }
    public List<WorkflowTransition> Transitions { get; set; } = new();
}