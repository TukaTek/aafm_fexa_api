using Fexa.ApiClient.Models;

namespace Fexa.ApiClient.Services;

public interface IDocumentService
{
    Task<DocumentUploadResponse> AddDocumentToWorkOrderAsync(
        int workOrderId,
        int documentTypeId,
        string description,
        Stream fileStream,
        string fileName,
        string? contentType = null,
        CancellationToken cancellationToken = default);
    
    Task<DocumentUploadResponse> AddDocumentToWorkOrderAsync(
        int workOrderId,
        int documentTypeId,
        string description,
        byte[] fileBytes,
        string fileName,
        string? contentType = null,
        CancellationToken cancellationToken = default);
    
    Task<DocumentUploadResponse> AddDocumentToWorkOrderAsync(
        DocumentUploadRequest request,
        CancellationToken cancellationToken = default);
}