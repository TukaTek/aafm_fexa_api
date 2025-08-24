using Fexa.ApiClient.Models;

namespace Fexa.ApiClient.Services;

public interface IClientService
{
    Task<PagedResponse<Client>> GetClientsAsync(QueryParameters? parameters = null, CancellationToken cancellationToken = default);
    Task<Client?> GetClientAsync(int id, CancellationToken cancellationToken = default);
    
    // Pagination helper - fetch all pages
    Task<List<Client>> GetAllClientsAsync(QueryParameters? baseParameters = null, int maxPages = 10, CancellationToken cancellationToken = default);
}