using Microsoft.Extensions.Logging;
using Fexa.ApiClient.Models;
using System.Text.Json;

namespace Fexa.ApiClient.Services;

public class NoteService : INoteService
{
    private readonly IFexaApiService _apiService;
    private readonly ILogger<NoteService> _logger;
    private const string BaseEndpoint = "/api/ev1/notes";

    public NoteService(IFexaApiService apiService, ILogger<NoteService> logger)
    {
        _apiService = apiService ?? throw new ArgumentNullException(nameof(apiService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<PagedResponse<Note>> GetNotesAsync(QueryParameters? parameters = null, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting notes with parameters: {Parameters}", parameters);
        
        var queryParams = parameters ?? new QueryParameters();
        var queryString = BuildQueryString(queryParams);
        var endpoint = $"{BaseEndpoint}{queryString}";
        
        var response = await _apiService.GetAsync<NotesResponse>(endpoint, cancellationToken);
        
        return ConvertToPagedResponse(response, queryParams.Start, queryParams.Limit);
    }

    public async Task<Note> GetNoteAsync(int noteId, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting note with ID: {NoteId}", noteId);
        
        var endpoint = $"{BaseEndpoint}/{noteId}";
        var response = await _apiService.GetAsync<SingleNoteResponse>(endpoint, cancellationToken);
        
        if (response?.Note == null)
        {
            throw new InvalidOperationException($"Note with ID {noteId} not found");
        }
        
        return response.Note;
    }

    public async Task<PagedResponse<Note>> GetNotesByObjectAsync(string objectType, int objectId, QueryParameters? parameters = null, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting notes for object type: {ObjectType}, ID: {ObjectId}", objectType, objectId);
        
        var queryParams = parameters ?? new QueryParameters();
        queryParams.Filters = queryParams.Filters ?? new List<FexaFilter>();
        // Use notable_id for the object ID filter - this is the main filter needed
        queryParams.Filters.Add(new FexaFilter("notable_id", objectId));
        
        // The API seems to work better with just notable_id, without notable_type
        // Based on testing, adding notable_type causes "invalid_request" errors
        // So we'll only use notable_id for filtering
        
        return await GetNotesAsync(queryParams, cancellationToken);
    }

    public async Task<PagedResponse<Note>> GetNotesByUserAsync(int userId, QueryParameters? parameters = null, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting notes by user: {UserId}", userId);
        
        var queryParams = parameters ?? new QueryParameters();
        queryParams.Filters = queryParams.Filters ?? new List<FexaFilter>();
        queryParams.Filters.Add(new FexaFilter("created_by", userId));
        
        return await GetNotesAsync(queryParams, cancellationToken);
    }

    public async Task<PagedResponse<Note>> GetNotesByTypeAsync(string noteType, QueryParameters? parameters = null, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting notes by type: {NoteType}", noteType);
        
        var queryParams = parameters ?? new QueryParameters();
        queryParams.Filters = queryParams.Filters ?? new List<FexaFilter>();
        queryParams.Filters.Add(new FexaFilter("note_type", noteType));
        
        return await GetNotesAsync(queryParams, cancellationToken);
    }

    public async Task<PagedResponse<Note>> GetNotesByDateRangeAsync(DateTime startDate, DateTime endDate, QueryParameters? parameters = null, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting notes between {StartDate} and {EndDate}", startDate, endDate);
        
        var queryParams = parameters ?? new QueryParameters();
        queryParams.Filters = queryParams.Filters ?? new List<FexaFilter>();
        queryParams.Filters.Add(new FexaFilter(
            "created_at",
            new[] { 
                startDate.ToString("yyyy-MM-dd HH:mm:ss"), 
                endDate.ToString("yyyy-MM-dd HH:mm:ss") 
            },
            FilterOperators.Between
        ));
        
        return await GetNotesAsync(queryParams, cancellationToken);
    }

    public async Task<List<Note>> GetAllNotesAsync(QueryParameters? baseParameters = null, int maxPages = 10, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching all notes (up to {MaxPages} pages)", maxPages);
        
        var allNotes = new List<Note>();
        var pageSize = baseParameters?.Limit ?? 100;
        var currentPage = 0;
        var hasMoreData = true;
        
        while (hasMoreData && currentPage < maxPages)
        {
            var parameters = new QueryParameters
            {
                Start = currentPage * pageSize,
                Limit = pageSize,
                SortBy = baseParameters?.SortBy,
                SortDescending = baseParameters?.SortDescending ?? false,
                Filters = baseParameters?.Filters
            };
            
            var response = await GetNotesAsync(parameters, cancellationToken);
            
            if (response.Data != null && response.Data.Any())
            {
                allNotes.AddRange(response.Data);
                _logger.LogDebug("Fetched page {Page} with {Count} notes. Total so far: {Total}", 
                    currentPage + 1, response.Data.Count(), allNotes.Count);
            }
            
            hasMoreData = response.Data != null && 
                         response.Data.Count() == pageSize && 
                         (response.TotalCount == 0 || allNotes.Count < response.TotalCount);
            
            currentPage++;
        }
        
        _logger.LogInformation("Fetched {Total} notes across {Pages} pages", 
            allNotes.Count, currentPage);
        
        return allNotes;
    }

    public async Task<List<Note>> GetAllNotesByObjectAsync(string objectType, int objectId, QueryParameters? baseParameters = null, int maxPages = 10, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching all notes for object type: {ObjectType}, ID: {ObjectId} (up to {MaxPages} pages)", 
            objectType, objectId, maxPages);
        
        var allNotes = new List<Note>();
        var pageSize = baseParameters?.Limit ?? 100;
        var currentPage = 0;
        var hasMoreData = true;
        
        while (hasMoreData && currentPage < maxPages)
        {
            var parameters = new QueryParameters
            {
                Start = currentPage * pageSize,
                Limit = pageSize,
                SortBy = baseParameters?.SortBy,
                SortDescending = baseParameters?.SortDescending ?? false,
                Filters = baseParameters?.Filters
            };
            
            var response = await GetNotesByObjectAsync(objectType, objectId, parameters, cancellationToken);
            
            if (response.Data != null && response.Data.Any())
            {
                allNotes.AddRange(response.Data);
                _logger.LogDebug("Fetched page {Page} with {Count} notes. Total so far: {Total}", 
                    currentPage + 1, response.Data.Count(), allNotes.Count);
            }
            
            hasMoreData = response.Data != null && 
                         response.Data.Count() == pageSize && 
                         (response.TotalCount == 0 || allNotes.Count < response.TotalCount);
            
            currentPage++;
        }
        
        _logger.LogInformation("Fetched {Total} notes for object type: {ObjectType}, ID: {ObjectId} across {Pages} pages", 
            allNotes.Count, objectType, objectId, currentPage);
        
        return allNotes;
    }

    public async Task<Note> CreateNoteAsync(CreateNoteRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating note for notable ID: {NotableId}", request.NotableId);
        
        // Transform to API format
        var apiRequest = new CreateNoteApiRequest
        {
            Notes = new CreateNoteApiData
            {
                Note = request.Content,
                Visibility = request.Visibility ?? "all",
                ActionRequired = request.ActionRequired,
                NotableId = request.NotableId,
                // For generic notes, still use note_type_id
                // Use CreateNoteForWorkOrderAsync for work order specific notes
                NoteTypeId = request.NoteTypeId ?? 2
            }
        };
        
        // The API returns {"notes":[{...}]} not {"note":{...}}
        var response = await _apiService.PostAsync<NotesResponse>(BaseEndpoint, apiRequest, cancellationToken);
        
        if (response?.Notes == null || !response.Notes.Any())
        {
            throw new InvalidOperationException("Failed to create note");
        }
        
        return response.Notes.First();
    }
    
    public async Task<Note> CreateNoteForWorkOrderAsync(int workOrderId, string content, string? visibility = "all", bool actionRequired = false, int? noteTypeId = 2, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating note for WorkOrder: {WorkOrderId}", workOrderId);
        
        // For work order notes, we need to use type instead of note_type_id
        var apiRequest = new CreateNoteApiRequest
        {
            Notes = new CreateNoteApiData
            {
                Note = content,
                Visibility = visibility ?? "all",
                ActionRequired = actionRequired,
                NotableId = workOrderId,
                Type = "Notes::WorkorderNote"  // Use type instead of note_type_id for work orders
            }
        };
        
        // The API returns {"notes":[{...}]} not {"note":{...}}
        var response = await _apiService.PostAsync<NotesResponse>(BaseEndpoint, apiRequest, cancellationToken);
        
        if (response?.Notes == null || !response.Notes.Any())
        {
            throw new InvalidOperationException("Failed to create note");
        }
        
        return response.Notes.First();
    }

    public async Task<Note> UpdateNoteAsync(int noteId, UpdateNoteRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating note: {NoteId}", noteId);
        
        var endpoint = $"{BaseEndpoint}/{noteId}";
        var response = await _apiService.PutAsync<SingleNoteResponse>(endpoint, request, cancellationToken);
        
        if (response?.Note == null)
        {
            throw new InvalidOperationException($"Failed to update note {noteId}");
        }
        
        return response.Note;
    }

    public async Task DeleteNoteAsync(int noteId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Deleting note: {NoteId}", noteId);
        
        var endpoint = $"{BaseEndpoint}/{noteId}";
        await _apiService.DeleteAsync<object>(endpoint, cancellationToken);
    }

    private string BuildQueryString(QueryParameters parameters)
    {
        var queryDict = new Dictionary<string, string>
        {
            ["start"] = parameters.Start.ToString(),
            ["limit"] = parameters.Limit.ToString()
        };
        
        if (!string.IsNullOrWhiteSpace(parameters.SortBy))
            queryDict["sortBy"] = parameters.SortBy;
            
        if (parameters.SortDescending)
            queryDict["sortDesc"] = "true";
        
        if (parameters.Filters?.Any() == true)
        {
            // Notes API uses a different filter format: filter=[{"property":"field","value":"value"}]
            var filters = parameters.Filters.Select(f => new Dictionary<string, object>
            {
                ["property"] = f.Property,
                ["value"] = f.Value
            }).ToList();
            
            var filterJson = JsonSerializer.Serialize(filters);
            queryDict["filter"] = filterJson;  // Note: "filter" not "filters"
        }
        
        var queryString = string.Join("&", queryDict.Select(kvp => $"{kvp.Key}={Uri.EscapeDataString(kvp.Value)}"));
        return string.IsNullOrEmpty(queryString) ? "" : $"?{queryString}";
    }

    private PagedResponse<Note> ConvertToPagedResponse(NotesResponse response, int start, int limit)
    {
        var pagedResponse = new PagedResponse<Note>
        {
            Success = true,
            Data = response.Notes,
            TotalCount = response.Notes?.Count ?? 0,
            Page = start / limit + 1,
            PageSize = limit
        };
        
        pagedResponse.TotalPages = (int)Math.Ceiling(pagedResponse.TotalCount / (double)pagedResponse.PageSize);
        
        return pagedResponse;
    }
}
