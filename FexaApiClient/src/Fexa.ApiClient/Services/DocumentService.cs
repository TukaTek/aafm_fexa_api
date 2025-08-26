using System.Net.Http.Headers;
using System.Text.Json;
using Fexa.ApiClient.Models;
using Microsoft.Extensions.Logging;

namespace Fexa.ApiClient.Services;

public class DocumentService : IDocumentService
{
    private readonly IFexaApiService _apiService;
    private readonly ILogger<DocumentService> _logger;

    public DocumentService(IFexaApiService apiService, ILogger<DocumentService> logger)
    {
        _apiService = apiService;
        _logger = logger;
    }

    public async Task<DocumentUploadResponse> AddDocumentToWorkOrderAsync(
        int workOrderId,
        int documentTypeId,
        string description,
        Stream fileStream,
        string fileName,
        string? contentType = null,
        CancellationToken cancellationToken = default)
    {
        var request = new DocumentUploadRequest
        {
            WorkOrderId = workOrderId,
            DocumentTypeId = documentTypeId,
            Description = description,
            FileStream = fileStream,
            FileName = fileName,
            ContentType = contentType
        };

        return await AddDocumentToWorkOrderAsync(request, cancellationToken);
    }

    public async Task<DocumentUploadResponse> AddDocumentToWorkOrderAsync(
        int workOrderId,
        int documentTypeId,
        string description,
        byte[] fileBytes,
        string fileName,
        string? contentType = null,
        CancellationToken cancellationToken = default)
    {
        var request = new DocumentUploadRequest
        {
            WorkOrderId = workOrderId,
            DocumentTypeId = documentTypeId,
            Description = description,
            FileBytes = fileBytes,
            FileName = fileName,
            ContentType = contentType
        };

        return await AddDocumentToWorkOrderAsync(request, cancellationToken);
    }

    public async Task<DocumentUploadResponse> AddDocumentToWorkOrderAsync(
        DocumentUploadRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Adding document to work order {WorkOrderId} with document type {DocumentTypeId}",
                request.WorkOrderId, request.DocumentTypeId);

            // Create multipart form content
            using var formContent = new MultipartFormDataContent();

            // Add form fields with exact Fexa field names
            formContent.Add(new StringContent(request.DocumentTypeId.ToString()), "documents[document_type_id]");
            formContent.Add(new StringContent(request.Description ?? string.Empty), "documents[description]");
            formContent.Add(new StringContent("Workorders::Workorder"), "documents[object_documents][0][attachable_type]");
            formContent.Add(new StringContent(request.WorkOrderId.ToString()), "documents[object_documents][0][attachable_id]");

            // Add file content
            HttpContent fileContent;
            if (request.FileStream != null)
            {
                fileContent = new StreamContent(request.FileStream);
            }
            else if (request.FileBytes != null)
            {
                fileContent = new ByteArrayContent(request.FileBytes);
            }
            else
            {
                throw new ArgumentException("Either FileStream or FileBytes must be provided");
            }

            // Set content type for the file
            if (!string.IsNullOrEmpty(request.ContentType))
            {
                fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse(request.ContentType);
            }
            else
            {
                // Try to determine content type from file extension
                var contentType = GetContentTypeFromFileName(request.FileName);
                if (!string.IsNullOrEmpty(contentType))
                {
                    fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse(contentType);
                }
            }

            // Add file to form with field name "documents[file]"
            formContent.Add(fileContent, "documents[file]", request.FileName);

            // Create HTTP request
            var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/api/ev1/documents")
            {
                Content = formContent
            };

            // Send request using IFexaApiService.SendAsync
            var response = await _apiService.SendAsync(httpRequest, cancellationToken);
            
            // Read response content
            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogDebug("Document upload response: {Response}", responseContent);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Document upload failed with status {StatusCode}: {Response}",
                    response.StatusCode, responseContent);
                
                return new DocumentUploadResponse
                {
                    Success = false,
                    Message = $"Upload failed with status {response.StatusCode}",
                    Errors = responseContent
                };
            }

            // Parse response
            var uploadResponse = JsonSerializer.Deserialize<DocumentUploadResponse>(responseContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (uploadResponse == null)
            {
                return new DocumentUploadResponse
                {
                    Success = false,
                    Message = "Failed to parse response",
                    Errors = responseContent
                };
            }

            // Check for success flag or presence of document
            if (uploadResponse.Document != null || uploadResponse.Documents?.Count > 0)
            {
                uploadResponse.Success = true;
                var documentId = uploadResponse.Document?.Id ?? uploadResponse.Documents?.FirstOrDefault()?.Id;
                _logger.LogInformation("Successfully uploaded document {DocumentId} to work order {WorkOrderId}",
                    documentId, request.WorkOrderId);
            }

            return uploadResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading document to work order {WorkOrderId}", request.WorkOrderId);
            
            return new DocumentUploadResponse
            {
                Success = false,
                Message = "An error occurred while uploading the document",
                Errors = ex.Message
            };
        }
    }

    private string? GetContentTypeFromFileName(string fileName)
    {
        if (string.IsNullOrEmpty(fileName))
            return null;

        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return extension switch
        {
            ".pdf" => "application/pdf",
            ".doc" => "application/msword",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            ".xls" => "application/vnd.ms-excel",
            ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            ".png" => "image/png",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".gif" => "image/gif",
            ".txt" => "text/plain",
            ".csv" => "text/csv",
            ".zip" => "application/zip",
            _ => "application/octet-stream"
        };
    }
}