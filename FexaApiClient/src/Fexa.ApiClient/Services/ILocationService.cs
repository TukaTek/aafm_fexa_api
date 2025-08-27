using Fexa.ApiClient.Models;

namespace Fexa.ApiClient.Services;

public interface ILocationService
{
    Task<List<LocationDto>> GetLocationsByClientAsync(int clientId, CancellationToken cancellationToken = default);
    Task<LocationDto> GetLocationAsync(int locationId, CancellationToken cancellationToken = default);
    Task<List<LocationDto>> GetAllLocationsAsync(CancellationToken cancellationToken = default);
    Task<List<LocationDto>> GetActiveLocationsAsync(CancellationToken cancellationToken = default);
    Task<List<LocationDto>> SearchLocationsAsync(string searchTerm, int? clientId = null, CancellationToken cancellationToken = default);
}