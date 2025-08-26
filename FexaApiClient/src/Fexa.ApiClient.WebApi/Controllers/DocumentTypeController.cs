using Microsoft.AspNetCore.Mvc;
using Fexa.ApiClient.Services;
using Fexa.ApiClient.Models;

namespace Fexa.ApiClient.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DocumentTypeController : ControllerBase
{
    private readonly IDocumentTypeService _documentTypeService;
    private readonly ILogger<DocumentTypeController> _logger;

    public DocumentTypeController(
        IDocumentTypeService documentTypeService,
        ILogger<DocumentTypeController> logger)
    {
        _documentTypeService = documentTypeService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<List<DocumentTypeDto>>> GetAllDocumentTypes()
    {
        try
        {
            _logger.LogInformation("Getting all document types");
            var documentTypes = await _documentTypeService.GetAllDocumentTypesAsync();
            var dtos = documentTypes.Select(dt => new DocumentTypeDto
            {
                Id = dt.Id,
                Name = dt.Name,
                Description = dt.Description
            }).ToList();
            return Ok(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all document types");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpGet("active")]
    public async Task<ActionResult<List<DocumentTypeDto>>> GetActiveDocumentTypes()
    {
        try
        {
            _logger.LogInformation("Getting active document types");
            var documentTypes = await _documentTypeService.GetActiveDocumentTypesAsync();
            var dtos = documentTypes.Select(dt => new DocumentTypeDto
            {
                Id = dt.Id,
                Name = dt.Name,
                Description = dt.Description
            }).ToList();
            return Ok(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active document types");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<DocumentTypeDto>> GetDocumentType(int id)
    {
        try
        {
            _logger.LogInformation("Getting document type {Id}", id);
            var documentType = await _documentTypeService.GetDocumentTypeByIdAsync(id);
            if (documentType == null)
            {
                return NotFound();
            }
            var dto = new DocumentTypeDto
            {
                Id = documentType.Id,
                Name = documentType.Name,
                Description = documentType.Description
            };
            return Ok(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting document type {Id}", id);
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpGet("byname/{name}")]
    public async Task<ActionResult<DocumentTypeDto>> GetDocumentTypeByName(string name)
    {
        try
        {
            _logger.LogInformation("Getting document type by name {Name}", name);
            var documentType = await _documentTypeService.GetDocumentTypeByNameAsync(name);
            if (documentType == null)
            {
                return NotFound();
            }
            var dto = new DocumentTypeDto
            {
                Id = documentType.Id,
                Name = documentType.Name,
                Description = documentType.Description
            };
            return Ok(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting document type by name {Name}", name);
            return StatusCode(500, new { error = ex.Message });
        }
    }
}