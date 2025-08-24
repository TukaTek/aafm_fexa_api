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

public class VendorFunctions
{
    private readonly IVendorService _vendorService;
    private readonly ILogger<VendorFunctions> _logger;

    public VendorFunctions(IVendorService vendorService, ILogger<VendorFunctions> logger)
    {
        _vendorService = vendorService ?? throw new ArgumentNullException(nameof(vendorService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets all vendors
    /// </summary>
    /// <param name="req">HTTP request</param>
    /// <returns>List of vendors</returns>
    [Function("GetVendors")]
    [OpenApiOperation(operationId: "GetVendors", tags: new[] { "Vendors" }, Summary = "Get all vendors", Description = "Retrieves a paginated list of all vendors (subcontractors).")]
    [OpenApiParameter(name: "start", In = ParameterLocation.Query, Required = false, Type = typeof(int), Description = "Starting position for pagination")]
    [OpenApiParameter(name: "limit", In = ParameterLocation.Query, Required = false, Type = typeof(int), Description = "Number of records to return")]
    [OpenApiParameter(name: "sortBy", In = ParameterLocation.Query, Required = false, Type = typeof(string), Description = "Field to sort by")]
    [OpenApiParameter(name: "sortDesc", In = ParameterLocation.Query, Required = false, Type = typeof(bool), Description = "Sort in descending order")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(PagedResponse<Vendor>), Description = "List of vendors")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(ErrorResponse), Description = "Internal server error")]
    public async Task<HttpResponseData> GetVendors(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "vendors")] 
        HttpRequestData req)
    {
        try
        {
            _logger.LogInformation("Getting all vendors");
            
            var parameters = ParseQueryParameters(req);
            var vendors = await _vendorService.GetVendorsAsync(parameters);
            
            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(vendors);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting vendors");
            var response = req.CreateResponse(HttpStatusCode.InternalServerError);
            await response.WriteAsJsonAsync(new ErrorResponse { Error = ex.Message });
            return response;
        }
    }

    /// <summary>
    /// Gets a specific vendor by ID
    /// </summary>
    /// <param name="req">HTTP request</param>
    /// <param name="id">Vendor ID</param>
    /// <returns>Vendor details</returns>
    [Function("GetVendor")]
    [OpenApiOperation(operationId: "GetVendor", tags: new[] { "Vendors" }, Summary = "Get vendor by ID", Description = "Retrieves details of a specific vendor.")]
    [OpenApiParameter(name: "id", In = ParameterLocation.Path, Required = true, Type = typeof(int), Description = "The vendor ID")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(Vendor), Description = "Vendor details")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.NotFound, contentType: "application/json", bodyType: typeof(ErrorResponse), Description = "Vendor not found")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(ErrorResponse), Description = "Internal server error")]
    public async Task<HttpResponseData> GetVendor(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "vendors/{id}")] 
        HttpRequestData req,
        int id)
    {
        try
        {
            _logger.LogInformation("Getting vendor {VendorId}", id);
            
            var vendor = await _vendorService.GetVendorAsync(id);
            
            if (vendor == null)
            {
                var notFoundResponse = req.CreateResponse(HttpStatusCode.NotFound);
                await notFoundResponse.WriteAsJsonAsync(new ErrorResponse { Error = "Vendor not found" });
                return notFoundResponse;
            }
            
            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(vendor);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting vendor {VendorId}", id);
            var response = req.CreateResponse(HttpStatusCode.InternalServerError);
            await response.WriteAsJsonAsync(new ErrorResponse { Error = ex.Message });
            return response;
        }
    }

    /// <summary>
    /// Gets all vendors (fetches all pages)
    /// </summary>
    /// <param name="req">HTTP request</param>
    /// <returns>All vendors</returns>
    [Function("GetAllVendors")]
    [OpenApiOperation(operationId: "GetAllVendors", tags: new[] { "Vendors" }, Summary = "Get all vendors (all pages)", Description = "Retrieves all vendors by fetching all pages.")]
    [OpenApiParameter(name: "maxPages", In = ParameterLocation.Query, Required = false, Type = typeof(int), Description = "Maximum number of pages to fetch (default: 10)")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(List<Vendor>), Description = "List of all vendors")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(ErrorResponse), Description = "Internal server error")]
    public async Task<HttpResponseData> GetAllVendors(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "vendors/all")] 
        HttpRequestData req)
    {
        try
        {
            _logger.LogInformation("Getting all vendors (all pages)");
            
            var maxPages = 10;
            if (req.Query["maxPages"] != null && int.TryParse(req.Query["maxPages"], out var mp))
            {
                maxPages = mp;
            }
            
            var vendors = await _vendorService.GetAllVendorsAsync(maxPages: maxPages);
            
            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(new
            {
                count = vendors.Count,
                vendors = vendors
            });
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all vendors");
            var response = req.CreateResponse(HttpStatusCode.InternalServerError);
            await response.WriteAsJsonAsync(new ErrorResponse { Error = ex.Message });
            return response;
        }
    }

    /// <summary>
    /// Gets active vendors only
    /// </summary>
    /// <param name="req">HTTP request</param>
    /// <returns>List of active vendors</returns>
    [Function("GetActiveVendors")]
    [OpenApiOperation(operationId: "GetActiveVendors", tags: new[] { "Vendors" }, Summary = "Get active vendors", Description = "Retrieves only active vendors.")]
    [OpenApiParameter(name: "start", In = ParameterLocation.Query, Required = false, Type = typeof(int), Description = "Starting position for pagination")]
    [OpenApiParameter(name: "limit", In = ParameterLocation.Query, Required = false, Type = typeof(int), Description = "Number of records to return")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(PagedResponse<Vendor>), Description = "List of active vendors")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(ErrorResponse), Description = "Internal server error")]
    public async Task<HttpResponseData> GetActiveVendors(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "vendors/active")] 
        HttpRequestData req)
    {
        try
        {
            _logger.LogInformation("Getting active vendors");
            
            var parameters = ParseQueryParameters(req);
            var vendors = await _vendorService.GetActiveVendorsAsync(parameters);
            
            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(vendors);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active vendors");
            var response = req.CreateResponse(HttpStatusCode.InternalServerError);
            await response.WriteAsJsonAsync(new ErrorResponse { Error = ex.Message });
            return response;
        }
    }

    /// <summary>
    /// Gets vendors by compliance status
    /// </summary>
    /// <param name="req">HTTP request</param>
    /// <param name="compliant">Compliance status</param>
    /// <returns>List of vendors with specified compliance status</returns>
    [Function("GetVendorsByCompliance")]
    [OpenApiOperation(operationId: "GetVendorsByCompliance", tags: new[] { "Vendors" }, Summary = "Get vendors by compliance status", Description = "Retrieves vendors filtered by compliance status.")]
    [OpenApiParameter(name: "compliant", In = ParameterLocation.Path, Required = true, Type = typeof(bool), Description = "Compliance status (true/false)")]
    [OpenApiParameter(name: "start", In = ParameterLocation.Query, Required = false, Type = typeof(int), Description = "Starting position for pagination")]
    [OpenApiParameter(name: "limit", In = ParameterLocation.Query, Required = false, Type = typeof(int), Description = "Number of records to return")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(PagedResponse<Vendor>), Description = "List of vendors")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(ErrorResponse), Description = "Internal server error")]
    public async Task<HttpResponseData> GetVendorsByCompliance(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "vendors/compliance/{compliant}")] 
        HttpRequestData req,
        bool compliant)
    {
        try
        {
            _logger.LogInformation("Getting vendors by compliance status: {Compliant}", compliant);
            
            var parameters = ParseQueryParameters(req);
            var vendors = await _vendorService.GetVendorsByComplianceStatusAsync(compliant, parameters);
            
            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(vendors);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting vendors by compliance status");
            var response = req.CreateResponse(HttpStatusCode.InternalServerError);
            await response.WriteAsJsonAsync(new ErrorResponse { Error = ex.Message });
            return response;
        }
    }

    /// <summary>
    /// Gets assignable vendors only
    /// </summary>
    /// <param name="req">HTTP request</param>
    /// <returns>List of assignable vendors</returns>
    [Function("GetAssignableVendors")]
    [OpenApiOperation(operationId: "GetAssignableVendors", tags: new[] { "Vendors" }, Summary = "Get assignable vendors", Description = "Retrieves only vendors that can be assigned to work orders.")]
    [OpenApiParameter(name: "start", In = ParameterLocation.Query, Required = false, Type = typeof(int), Description = "Starting position for pagination")]
    [OpenApiParameter(name: "limit", In = ParameterLocation.Query, Required = false, Type = typeof(int), Description = "Number of records to return")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(PagedResponse<Vendor>), Description = "List of assignable vendors")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(ErrorResponse), Description = "Internal server error")]
    public async Task<HttpResponseData> GetAssignableVendors(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "vendors/assignable")] 
        HttpRequestData req)
    {
        try
        {
            _logger.LogInformation("Getting assignable vendors");
            
            var parameters = ParseQueryParameters(req);
            var vendors = await _vendorService.GetAssignableVendorsAsync(parameters);
            
            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(vendors);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting assignable vendors");
            var response = req.CreateResponse(HttpStatusCode.InternalServerError);
            await response.WriteAsJsonAsync(new ErrorResponse { Error = ex.Message });
            return response;
        }
    }

    [Function("GetVendorsByWorkOrder")]
    [OpenApiOperation(operationId: "GetVendorsByWorkOrder", tags: new[] { "Vendors" }, Summary = "Get vendors assigned to a work order", Description = "Returns all vendors assigned to a specific work order")]
    [OpenApiParameter(name: "workOrderId", In = ParameterLocation.Path, Required = true, Type = typeof(int), Description = "The work order ID")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(List<Vendor>), Description = "List of vendors assigned to the work order")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.NotFound, contentType: "application/json", bodyType: typeof(ErrorResponse), Description = "Work order not found")]
    public async Task<HttpResponseData> GetVendorsByWorkOrder(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "vendors/workorder/{workOrderId:int}")] HttpRequestData req,
        int workOrderId)
    {
        _logger.LogInformation("Getting vendors for work order {WorkOrderId}", workOrderId);
        
        var response = req.CreateResponse();
        response.Headers.Add("Content-Type", "application/json");
        
        try
        {
            var vendors = await _vendorService.GetVendorsByWorkOrderIdAsync(workOrderId);
            
            response.StatusCode = HttpStatusCode.OK;
            await response.WriteAsJsonAsync(vendors);
            return response;
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            _logger.LogWarning("Work order {WorkOrderId} not found", workOrderId);
            response.StatusCode = HttpStatusCode.NotFound;
            await response.WriteAsJsonAsync(new ErrorResponse { Error = $"Work order {workOrderId} not found" });
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting vendors for work order {WorkOrderId}", workOrderId);
            response.StatusCode = HttpStatusCode.InternalServerError;
            await response.WriteAsJsonAsync(new ErrorResponse { Error = ex.Message });
            return response;
        }
    }

    private QueryParameters ParseQueryParameters(HttpRequestData req)
    {
        var query = System.Web.HttpUtility.ParseQueryString(req.Url.Query);
        
        var parameters = new QueryParameters
        {
            Start = int.TryParse(query["start"], out var start) ? start : 0,
            Limit = int.TryParse(query["limit"], out var limit) ? limit : 100,
            SortBy = query["sortBy"],
            SortDescending = bool.TryParse(query["sortDesc"], out var sortDesc) && sortDesc
        };
        
        return parameters;
    }
}