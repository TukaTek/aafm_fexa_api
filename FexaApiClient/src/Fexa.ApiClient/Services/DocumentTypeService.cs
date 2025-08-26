using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Fexa.ApiClient.Models;

namespace Fexa.ApiClient.Services;

public class DocumentTypeService : IDocumentTypeService
{
    private readonly IFexaApiService _apiService;
    private readonly ILogger<DocumentTypeService> _logger;
    private readonly IMemoryCache _cache;
    private const string CACHE_KEY = "document_types_all";
    private readonly TimeSpan _cacheExpiration = TimeSpan.FromHours(1); // Cache for 1 hour since these don't change often

    public DocumentTypeService(
        IFexaApiService apiService,
        ILogger<DocumentTypeService> logger,
        IMemoryCache cache)
    {
        _apiService = apiService ?? throw new ArgumentNullException(nameof(apiService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
    }

    public async Task<List<DocumentType>> GetAllDocumentTypesAsync(CancellationToken cancellationToken = default)
    {
        // Check cache first
        if (_cache.TryGetValue(CACHE_KEY, out List<DocumentType>? cachedTypes))
        {
            _logger.LogDebug("Returning cached document types");
            return cachedTypes ?? new List<DocumentType>();
        }

        try
        {
            _logger.LogInformation("Fetching all document types from API");
            
            // Call the API endpoint
            var response = await _apiService.GetAsync<DocumentTypesResponse>("/api/ev1/document_types", cancellationToken);
            
            if (response?.DocumentTypes != null)
            {
                _logger.LogInformation("Successfully fetched {Count} document types", response.DocumentTypes.Count);
                
                // Cache the results
                _cache.Set(CACHE_KEY, response.DocumentTypes, _cacheExpiration);
                
                return response.DocumentTypes;
            }
            
            _logger.LogWarning("No document types returned from API");
            return new List<DocumentType>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching document types");
            throw;
        }
    }

    public async Task<List<DocumentType>> GetActiveDocumentTypesAsync(CancellationToken cancellationToken = default)
    {
        var allTypes = await GetAllDocumentTypesAsync(cancellationToken);
        return allTypes.Where(dt => dt.Active).ToList();
    }

    public async Task<DocumentType?> GetDocumentTypeByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var allTypes = await GetAllDocumentTypesAsync(cancellationToken);
        return allTypes.FirstOrDefault(dt => dt.Id == id);
    }

    public async Task<DocumentType?> GetDocumentTypeByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(name))
            return null;
            
        var allTypes = await GetAllDocumentTypesAsync(cancellationToken);
        return allTypes.FirstOrDefault(dt => 
            string.Equals(dt.Name, name, StringComparison.OrdinalIgnoreCase));
    }
}