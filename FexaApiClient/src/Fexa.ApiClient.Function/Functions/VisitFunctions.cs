using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System.Net;
using Fexa.ApiClient.Services;
using Fexa.ApiClient.Models;

namespace Fexa.ApiClient.Function.Functions;

public class VisitFunctions
{
    private readonly IVisitService _visitService;
    private readonly ILogger<VisitFunctions> _logger;

    public VisitFunctions(IVisitService visitService, ILogger<VisitFunctions> logger)
    {
        _visitService = visitService;
        _logger = logger;
    }

    /// <summary>
    /// Gets visits for a specific work order
    /// </summary>
    [Function("GetVisitsByWorkOrder")]
    [OpenApiOperation(operationId: "GetVisitsByWorkOrder", tags: new[] { "Visits" }, Summary = "Get visits by work order", Description = "Retrieves all visits associated with a specific work order.")]
    [OpenApiParameter(name: "workOrderId", In = ParameterLocation.Path, Required = true, Type = typeof(int), Description = "The work order ID")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(PagedResponse<Visit>), Description = "List of visits")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(ErrorResponse), Description = "Internal server error")]
    public async Task<HttpResponseData> GetVisitsByWorkOrder(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "visits/workorder/{workOrderId}")] 
        HttpRequestData req,
        int workOrderId)
    {
        try
        {
            _logger.LogInformation("Getting visits for work order {WorkOrderId}", workOrderId);
            
            var visits = await _visitService.GetVisitsByWorkOrderAsync(workOrderId);
            
            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(visits);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting visits for work order {WorkOrderId}", workOrderId);
            var response = req.CreateResponse(HttpStatusCode.InternalServerError);
            await response.WriteAsJsonAsync(new { error = ex.Message });
            return response;
        }
    }

    /// <summary>
    /// Gets visits within a date range
    /// </summary>
    [Function("GetVisitsByDateRange")]
    [OpenApiOperation(operationId: "GetVisitsByDateRange", tags: new[] { "Visits" }, Summary = "Get visits by date range", Description = "Retrieves all visits within the specified date range.")]
    [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(DateRangeRequest), Required = true, Description = "Date range filter")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(PagedResponse<Visit>), Description = "List of visits")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: "application/json", bodyType: typeof(ErrorResponse), Description = "Invalid date range")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(ErrorResponse), Description = "Internal server error")]
    public async Task<HttpResponseData> GetVisitsByDateRange(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "visits/daterange")] 
        HttpRequestData req)
    {
        try
        {
            var requestBody = await req.ReadAsStringAsync();
            var dateRange = System.Text.Json.JsonSerializer.Deserialize<DateRangeRequest>(requestBody ?? "{}");
            
            if (dateRange == null || dateRange.StartDate == default || dateRange.EndDate == default)
            {
                var badRequest = req.CreateResponse(HttpStatusCode.BadRequest);
                await badRequest.WriteAsJsonAsync(new { error = "Invalid date range" });
                return badRequest;
            }
            
            _logger.LogInformation("Getting visits from {StartDate} to {EndDate}", 
                dateRange.StartDate, dateRange.EndDate);
            
            var visits = await _visitService.GetVisitsByDateRangeAsync(
                dateRange.StartDate, 
                dateRange.EndDate);
            
            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(visits);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting visits by date range");
            var response = req.CreateResponse(HttpStatusCode.InternalServerError);
            await response.WriteAsJsonAsync(new { error = ex.Message });
            return response;
        }
    }

    /// <summary>
    /// Gets a specific visit by ID
    /// </summary>
    [Function("GetVisit")]
    [OpenApiOperation(operationId: "GetVisit", tags: new[] { "Visits" }, Summary = "Get visit by ID", Description = "Retrieves details of a specific visit.")]
    [OpenApiParameter(name: "id", In = ParameterLocation.Path, Required = true, Type = typeof(int), Description = "The visit ID")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(Visit), Description = "Visit details")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.NotFound, contentType: "application/json", bodyType: typeof(ErrorResponse), Description = "Visit not found")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(ErrorResponse), Description = "Internal server error")]
    public async Task<HttpResponseData> GetVisit(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "visits/{id}")] 
        HttpRequestData req,
        int id)
    {
        try
        {
            _logger.LogInformation("Getting visit {VisitId}", id);
            
            var visit = await _visitService.GetVisitAsync(id);
            
            if (visit == null)
            {
                var notFoundResponse = req.CreateResponse(HttpStatusCode.NotFound);
                await notFoundResponse.WriteAsJsonAsync(new { error = "Visit not found" });
                return notFoundResponse;
            }
            
            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(visit);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting visit {VisitId}", id);
            var response = req.CreateResponse(HttpStatusCode.InternalServerError);
            await response.WriteAsJsonAsync(new { error = ex.Message });
            return response;
        }
    }
}

/// <summary>
/// Request model for date range filtering
/// </summary>
public class DateRangeRequest
{
    /// <summary>
    /// Start date for the range
    /// </summary>
    public DateTime StartDate { get; set; }
    
    /// <summary>
    /// End date for the range
    /// </summary>
    public DateTime EndDate { get; set; }
    
    /// <summary>
    /// Optional work order ID filter
    /// </summary>
    public int? WorkOrderId { get; set; }
}