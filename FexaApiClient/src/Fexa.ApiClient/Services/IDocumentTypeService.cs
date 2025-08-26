using Fexa.ApiClient.Models;

namespace Fexa.ApiClient.Services;

public interface IDocumentTypeService
{
    /// <summary>
    /// Gets all document types from the API (no paging needed)
    /// </summary>
    Task<List<DocumentType>> GetAllDocumentTypesAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets only active document types
    /// </summary>
    Task<List<DocumentType>> GetActiveDocumentTypesAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets a document type by ID (from cached list)
    /// </summary>
    Task<DocumentType?> GetDocumentTypeByIdAsync(int id, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets a document type by name (from cached list)
    /// </summary>
    Task<DocumentType?> GetDocumentTypeByNameAsync(string name, CancellationToken cancellationToken = default);
}