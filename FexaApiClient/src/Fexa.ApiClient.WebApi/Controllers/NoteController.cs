using Microsoft.AspNetCore.Mvc;
using Fexa.ApiClient.Services;
using Fexa.ApiClient.Models;

namespace Fexa.ApiClient.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NoteController : ControllerBase
{
    private readonly INoteService _noteService;
    private readonly ILogger<NoteController> _logger;

    public NoteController(
        INoteService noteService,
        ILogger<NoteController> logger)
    {
        _noteService = noteService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResponse<Note>>> GetNotes([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        try
        {
            _logger.LogInformation("Getting notes page {Page}", page);
            var parameters = new QueryParameters { Page = page, PageSize = pageSize };
            var notes = await _noteService.GetNotesAsync(parameters);
            return Ok(notes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting notes");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Note>> GetNote(int id)
    {
        try
        {
            _logger.LogInformation("Getting note {Id}", id);
            var note = await _noteService.GetNoteAsync(id);
            if (note == null)
            {
                return NotFound();
            }
            return Ok(note);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting note {Id}", id);
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpGet("workorder/{workOrderId}")]
    public async Task<ActionResult<PagedResponse<Note>>> GetNotesByWorkOrder(int workOrderId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        try
        {
            _logger.LogInformation("Getting notes for work order {WorkOrderId} page {Page}", workOrderId, page);
            var parameters = new QueryParameters { Page = page, PageSize = pageSize };
            var notes = await _noteService.GetNotesByObjectAsync("WorkOrder", workOrderId, parameters);
            return Ok(notes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting notes for work order {WorkOrderId}", workOrderId);
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpPost("workorder/{workOrderId}")]
    public async Task<ActionResult<Note>> CreateNoteForWorkOrder(int workOrderId, [FromBody] CreateNoteRequest request)
    {
        try
        {
            _logger.LogInformation("Creating note for work order {WorkOrderId}", workOrderId);
            
            if (string.IsNullOrWhiteSpace(request?.Text))
            {
                return BadRequest(new { error = "Note text is required" });
            }

            var note = await _noteService.CreateNoteForWorkOrderAsync(workOrderId, request.Text, visibility: request.IsPrivate == true ? "private" : "all");
            return Ok(note);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating note for work order {WorkOrderId}", workOrderId);
            return StatusCode(500, new { error = ex.Message });
        }
    }
}

public class CreateNoteRequest
{
    public string Text { get; set; } = string.Empty;
    public bool? IsPrivate { get; set; }
}