using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;
using System.Net;
using Fexa.ApiClient.Services;
using Fexa.ApiClient.Models;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Fexa.ApiClient.Function.Functions;

public class CreateWorkOrderNoteRequest
{
    public string Content { get; set; } = string.Empty;
    public string? Visibility { get; set; } = "all";
    public bool ActionRequired { get; set; } = false;
    public int? NoteTypeId { get; set; } = 2;
}

public class NoteFunctions
{
    private readonly INoteService _noteService;
    private readonly ILogger<NoteFunctions> _logger;

    public NoteFunctions(INoteService noteService, ILogger<NoteFunctions> logger)
    {
        _noteService = noteService ?? throw new ArgumentNullException(nameof(noteService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [Function("GetNotes")]
    [OpenApiOperation(operationId: "GetNotes", tags: new[] { "Notes" }, Summary = "Get notes", Description = "Retrieves a paginated list of notes with optional filtering")]
    [OpenApiParameter(name: "start", In = ParameterLocation.Query, Required = false, Type = typeof(int), Description = "Starting position for pagination")]
    [OpenApiParameter(name: "limit", In = ParameterLocation.Query, Required = false, Type = typeof(int), Description = "Number of records to return")]
    [OpenApiParameter(name: "sortBy", In = ParameterLocation.Query, Required = false, Type = typeof(string), Description = "Field to sort by")]
    [OpenApiParameter(name: "sortDesc", In = ParameterLocation.Query, Required = false, Type = typeof(bool), Description = "Sort in descending order")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(PagedResponse<Note>), Description = "The paginated list of notes")]
    [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.InternalServerError, Description = "Internal server error")]
    public async Task<HttpResponseData> GetNotes(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "notes")] HttpRequestData req)
    {
        try
        {
            var queryParams = ParseQueryParameters(req);
            var notes = await _noteService.GetNotesAsync(queryParams);
            
            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(notes);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving notes");
            return req.CreateResponse(HttpStatusCode.InternalServerError);
        }
    }

    [Function("GetNotesByWorkOrder")]
    [OpenApiOperation(operationId: "GetNotesByWorkOrder", tags: new[] { "Notes" }, Summary = "Get notes by work order", Description = "Retrieves notes associated with a specific work order")]
    [OpenApiParameter(name: "workOrderId", In = ParameterLocation.Path, Required = true, Type = typeof(int), Description = "Work Order ID")]
    [OpenApiParameter(name: "start", In = ParameterLocation.Query, Required = false, Type = typeof(int), Description = "Starting position for pagination")]
    [OpenApiParameter(name: "limit", In = ParameterLocation.Query, Required = false, Type = typeof(int), Description = "Number of records to return")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(PagedResponse<Note>), Description = "The paginated list of notes")]
    [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.InternalServerError, Description = "Internal server error")]
    public async Task<HttpResponseData> GetNotesByWorkOrder(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "workorders/{workOrderId:int}/notes")] HttpRequestData req,
        int workOrderId)
    {
        try
        {
            var queryParams = ParseQueryParameters(req);
            var notes = await _noteService.GetNotesByObjectAsync("WorkOrder", workOrderId, queryParams);
            
            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(notes);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving notes for work order {WorkOrderId}", workOrderId);
            return req.CreateResponse(HttpStatusCode.InternalServerError);
        }
    }

    [Function("CreateNoteForWorkOrder")]
    [OpenApiOperation(operationId: "CreateNoteForWorkOrder", tags: new[] { "Notes" }, Summary = "Create note for work order", Description = "Creates a new note attached to a specific work order")]
    [OpenApiParameter(name: "workOrderId", In = ParameterLocation.Path, Required = true, Type = typeof(int), Description = "Work Order ID")]
    [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(CreateWorkOrderNoteRequest), Description = "Note creation details")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Created, contentType: "application/json", bodyType: typeof(Note), Description = "The created note")]
    [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.BadRequest, Description = "Invalid request")]
    [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.InternalServerError, Description = "Internal server error")]
    public async Task<HttpResponseData> CreateNoteForWorkOrder(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "workorders/{workOrderId:int}/notes")] HttpRequestData req,
        int workOrderId)
    {
        try
        {
            var createRequest = await req.ReadFromJsonAsync<CreateWorkOrderNoteRequest>();
            if (createRequest == null || string.IsNullOrWhiteSpace(createRequest.Content))
            {
                return req.CreateResponse(HttpStatusCode.BadRequest);
            }
            
            var note = await _noteService.CreateNoteForWorkOrderAsync(
                workOrderId,
                createRequest.Content,
                createRequest.Visibility,
                createRequest.ActionRequired,
                createRequest.NoteTypeId
            );
            
            var response = req.CreateResponse(HttpStatusCode.Created);
            await response.WriteAsJsonAsync(note);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating note for work order {WorkOrderId}", workOrderId);
            return req.CreateResponse(HttpStatusCode.InternalServerError);
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
        
        // Parse filters if provided as JSON
        var filterJson = query["filters"];
        if (!string.IsNullOrEmpty(filterJson))
        {
            try
            {
                var filters = JsonSerializer.Deserialize<List<FexaFilter>>(filterJson);
                parameters.Filters = filters;
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(ex, "Failed to parse filters JSON");
            }
        }
        
        return parameters;
    }
}