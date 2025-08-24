using Fexa.ApiClient.Models;
using System.Text.Json.Serialization;

namespace Fexa.ApiClient.Services;

public interface INoteService
{
    Task<PagedResponse<Note>> GetNotesAsync(QueryParameters? parameters = null, CancellationToken cancellationToken = default);
    Task<Note> GetNoteAsync(int noteId, CancellationToken cancellationToken = default);
    Task<PagedResponse<Note>> GetNotesByObjectAsync(string objectType, int objectId, QueryParameters? parameters = null, CancellationToken cancellationToken = default);
    Task<PagedResponse<Note>> GetNotesByUserAsync(int userId, QueryParameters? parameters = null, CancellationToken cancellationToken = default);
    Task<PagedResponse<Note>> GetNotesByTypeAsync(string noteType, QueryParameters? parameters = null, CancellationToken cancellationToken = default);
    Task<PagedResponse<Note>> GetNotesByDateRangeAsync(DateTime startDate, DateTime endDate, QueryParameters? parameters = null, CancellationToken cancellationToken = default);
    
    // Pagination helpers - fetch all pages
    Task<List<Note>> GetAllNotesAsync(QueryParameters? baseParameters = null, int maxPages = 10, CancellationToken cancellationToken = default);
    Task<List<Note>> GetAllNotesByObjectAsync(string objectType, int objectId, QueryParameters? baseParameters = null, int maxPages = 10, CancellationToken cancellationToken = default);
    
    // CRUD operations
    Task<Note> CreateNoteAsync(CreateNoteRequest request, CancellationToken cancellationToken = default);
    Task<Note> CreateNoteForWorkOrderAsync(int workOrderId, string content, string? visibility = "all", bool actionRequired = false, int? noteTypeId = 2, CancellationToken cancellationToken = default);
    Task<Note> UpdateNoteAsync(int noteId, UpdateNoteRequest request, CancellationToken cancellationToken = default);
    Task DeleteNoteAsync(int noteId, CancellationToken cancellationToken = default);
}

public class CreateNoteRequest
{
    public string Content { get; set; } = string.Empty;
    public string? Visibility { get; set; } = "all";  // "all", "internal", "private"
    public bool ActionRequired { get; set; } = false;
    public int? NoteTypeId { get; set; } = 2;  // Default to general note type
    public int? NotableId { get; set; }  // WorkOrder ID or other object ID
}

// Internal API request format - matches what the Fexa API expects
internal class CreateNoteApiRequest
{
    [JsonPropertyName("notes")]
    public CreateNoteApiData Notes { get; set; } = new();
}

internal class CreateNoteApiData
{
    [JsonPropertyName("note")]
    public string Note { get; set; } = string.Empty;
    
    [JsonPropertyName("visibility")]
    public string? Visibility { get; set; }
    
    [JsonPropertyName("action_required")]
    public bool ActionRequired { get; set; }
    
    [JsonPropertyName("notable_id")]
    public int? NotableId { get; set; }
    
    [JsonPropertyName("note_type_id")]
    public int? NoteTypeId { get; set; }
}

public class UpdateNoteRequest
{
    public string Content { get; set; } = string.Empty;
    public string? NoteType { get; set; }
    public bool? IsPrivate { get; set; }
    public bool? IsInternal { get; set; }
}
