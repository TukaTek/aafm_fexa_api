using Microsoft.AspNetCore.Mvc;
using Fexa.ApiClient.Services;
using Fexa.ApiClient.Models;

namespace Fexa.ApiClient.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DocumentController : ControllerBase
{
    private readonly IDocumentService _documentService;
    private readonly ILogger<DocumentController> _logger;
    private const long MaxFileSize = 10 * 1024 * 1024; // 10MB default limit

    public DocumentController(
        IDocumentService documentService,
        ILogger<DocumentController> logger)
    {
        _documentService = documentService;
        _logger = logger;
    }

    /// <summary>
    /// Add a document to a work order
    /// </summary>
    /// <param name="workOrderId">The work order ID to attach the document to</param>
    /// <param name="documentTypeId">The document type ID from Fexa</param>
    /// <param name="description">Description of the document</param>
    /// <param name="file">The file to upload</param>
    /// <returns>Document upload response with document details</returns>
    [HttpPost("workorder/{workOrderId}")]
    [RequestSizeLimit(52428800)] // 50MB limit
    [RequestFormLimits(MultipartBodyLengthLimit = 52428800)] // 50MB limit
    [ProducesResponseType(typeof(DocumentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> AddDocumentToWorkOrder(
        int workOrderId,
        [FromForm] DocumentUploadFormRequest request)
    {
        try
        {
            // Validate inputs
            if (request?.File == null || request.File.Length == 0)
            {
                _logger.LogWarning("File upload attempted with no file for work order {WorkOrderId}", workOrderId);
                return BadRequest(new { error = "File is required" });
            }

            if (request.File.Length > MaxFileSize)
            {
                _logger.LogWarning("File upload attempted with oversized file ({FileSize} bytes) for work order {WorkOrderId}",
                    request.File.Length, workOrderId);
                return BadRequest(new { error = $"File size exceeds maximum allowed size of {MaxFileSize / 1024 / 1024}MB" });
            }

            if (string.IsNullOrWhiteSpace(request.Description))
            {
                _logger.LogWarning("File upload attempted without description for work order {WorkOrderId}", workOrderId);
                return BadRequest(new { error = "Description is required" });
            }

            // Validate file extension (optional security measure)
            var allowedExtensions = new[] { ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".png", ".jpg", ".jpeg", ".txt", ".csv" };
            var fileExtension = Path.GetExtension(request.File.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(fileExtension))
            {
                _logger.LogWarning("File upload attempted with unsupported file type {Extension} for work order {WorkOrderId}",
                    fileExtension, workOrderId);
                return BadRequest(new { error = $"File type '{fileExtension}' is not allowed" });
            }

            _logger.LogInformation("Processing document upload for work order {WorkOrderId}: {FileName} ({FileSize} bytes)",
                workOrderId, request.File.FileName, request.File.Length);

            // Upload document using service
            using var stream = request.File.OpenReadStream();
            var response = await _documentService.AddDocumentToWorkOrderAsync(
                workOrderId,
                request.DocumentTypeId,
                request.Description,
                stream,
                request.File.FileName,
                request.File.ContentType);

            if (!response.Success)
            {
                _logger.LogError("Document upload failed for work order {WorkOrderId}: {Errors}",
                    workOrderId, string.Join(", ", response.Errors ?? new List<string>()));
                return StatusCode(500, new 
                { 
                    error = response.Message ?? "Failed to upload document",
                    details = response.Errors?.ToString()
                });
            }

            // Map to DTO for response
            var document = response.Document ?? response.Documents?.FirstOrDefault();
            if (document != null)
            {
                var dto = new DocumentDto
                {
                    Id = document.Id,
                    DocumentTypeId = document.DocumentTypeId,
                    Description = document.Description,
                    FileName = document.FileName ?? document.Filename,
                    FileSize = document.FileSize,
                    ContentType = document.ContentType,
                    Url = document.Url,
                    CreatedAt = document.CreatedAt,
                    CreatedBy = document.CreatedBy
                };

                _logger.LogInformation("Successfully uploaded document {DocumentId} to work order {WorkOrderId}",
                    document.Id, workOrderId);

                return Ok(dto);
            }

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading document to work order {WorkOrderId}", workOrderId);
            return StatusCode(500, new { error = "An error occurred while processing the document upload" });
        }
    }

    /// <summary>
    /// Add a document to a work order using base64 encoded file
    /// </summary>
    /// <param name="workOrderId">The work order ID to attach the document to</param>
    /// <param name="request">The upload request with base64 encoded file</param>
    /// <returns>Document upload response with document details</returns>
    [HttpPost("workorder/{workOrderId}/base64")]
    [ProducesResponseType(typeof(DocumentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> AddDocumentToWorkOrderBase64(
        int workOrderId,
        [FromBody] DocumentUploadBase64Request request)
    {
        try
        {
            // Validate inputs
            if (string.IsNullOrWhiteSpace(request.FileBase64))
            {
                return BadRequest(new { error = "File content is required" });
            }

            if (string.IsNullOrWhiteSpace(request.FileName))
            {
                return BadRequest(new { error = "File name is required" });
            }

            if (string.IsNullOrWhiteSpace(request.Description))
            {
                return BadRequest(new { error = "Description is required" });
            }

            // Decode base64 file
            byte[] fileBytes;
            try
            {
                fileBytes = Convert.FromBase64String(request.FileBase64);
            }
            catch (FormatException)
            {
                return BadRequest(new { error = "Invalid base64 file content" });
            }

            if (fileBytes.Length > MaxFileSize)
            {
                return BadRequest(new { error = $"File size exceeds maximum allowed size of {MaxFileSize / 1024 / 1024}MB" });
            }

            _logger.LogInformation("Processing base64 document upload for work order {WorkOrderId}: {FileName} ({FileSize} bytes)",
                workOrderId, request.FileName, fileBytes.Length);

            // Upload document using service
            var response = await _documentService.AddDocumentToWorkOrderAsync(
                workOrderId,
                request.DocumentTypeId,
                request.Description,
                fileBytes,
                request.FileName,
                request.ContentType);

            if (!response.Success)
            {
                _logger.LogError("Document upload failed for work order {WorkOrderId}: {Errors}",
                    workOrderId, string.Join(", ", response.Errors ?? new List<string>()));
                return StatusCode(500, new 
                { 
                    error = response.Message ?? "Failed to upload document",
                    details = response.Errors?.ToString()
                });
            }

            // Map to DTO for response
            var document = response.Document ?? response.Documents?.FirstOrDefault();
            if (document != null)
            {
                var dto = new DocumentDto
                {
                    Id = document.Id,
                    DocumentTypeId = document.DocumentTypeId,
                    Description = document.Description,
                    FileName = document.FileName ?? document.Filename,
                    FileSize = document.FileSize,
                    ContentType = document.ContentType,
                    Url = document.Url,
                    CreatedAt = document.CreatedAt,
                    CreatedBy = document.CreatedBy
                };

                _logger.LogInformation("Successfully uploaded document {DocumentId} to work order {WorkOrderId}",
                    document.Id, workOrderId);

                return Ok(dto);
            }

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading base64 document to work order {WorkOrderId}", workOrderId);
            return StatusCode(500, new { error = "An error occurred while processing the document upload" });
        }
    }
}

public class DocumentUploadFormRequest
{
    public int DocumentTypeId { get; set; }
    public string Description { get; set; } = string.Empty;
    public IFormFile File { get; set; } = null!;
}

public class DocumentUploadBase64Request
{
    public int DocumentTypeId { get; set; }
    public string Description { get; set; } = string.Empty;
    public string FileBase64 { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string? ContentType { get; set; }
}