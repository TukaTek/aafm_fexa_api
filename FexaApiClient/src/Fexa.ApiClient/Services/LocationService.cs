using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Memory;
using Fexa.ApiClient.Models;
using System.Text.Json;
using System.Web;

namespace Fexa.ApiClient.Services;

public class LocationService : ILocationService
{
    private readonly IFexaApiService _apiService;
    private readonly IMemoryCache _cache;
    private readonly ILogger<LocationService> _logger;
    private const string BaseEndpoint = "/api/ev1/stores";
    private readonly TimeSpan _cacheExpiration = TimeSpan.FromHours(1);

    public LocationService(
        IFexaApiService apiService, 
        IMemoryCache cache,
        ILogger<LocationService> logger)
    {
        _apiService = apiService ?? throw new ArgumentNullException(nameof(apiService));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<List<LocationDto>> GetLocationsByClientAsync(int clientId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting locations for client {ClientId}", clientId);
        
        var cacheKey = $"locations_client_{clientId}";
        if (_cache.TryGetValue<List<LocationDto>>(cacheKey, out var cachedLocations))
        {
            _logger.LogDebug("Returning cached locations for client {ClientId}", clientId);
            return cachedLocations!;
        }
        
        // Build filter for occupied_by
        var filter = new[]
        {
            new Dictionary<string, object>
            {
                ["property"] = "occupied_by",
                ["value"] = clientId
            }
        };
        
        var filterJson = JsonSerializer.Serialize(filter);
        var encodedFilter = HttpUtility.UrlEncode(filterJson);
        var endpoint = $"{BaseEndpoint}?filter={encodedFilter}";
        
        var response = await _apiService.GetAsync<LocationsResponse>(endpoint, cancellationToken);
        var locations = ConvertToDtos(response?.Stores);
        
        _cache.Set(cacheKey, locations, _cacheExpiration);
        _logger.LogInformation("Retrieved and cached {Count} locations for client {ClientId}", locations.Count, clientId);
        
        return locations;
    }

    public async Task<LocationDto> GetLocationAsync(int locationId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting location {LocationId}", locationId);
        
        var cacheKey = $"location_{locationId}";
        if (_cache.TryGetValue<LocationDto>(cacheKey, out var cachedLocation))
        {
            _logger.LogDebug("Returning cached location {LocationId}", locationId);
            return cachedLocation!;
        }
        
        var endpoint = $"{BaseEndpoint}/{locationId}";
        var response = await _apiService.GetAsync<SingleLocationResponse>(endpoint, cancellationToken);
        
        if (response?.Store == null)
        {
            throw new InvalidOperationException($"Location {locationId} not found");
        }
        
        var locationDto = ConvertToDto(response.Store);
        _cache.Set(cacheKey, locationDto, _cacheExpiration);
        
        return locationDto;
    }

    public async Task<List<LocationDto>> GetAllLocationsAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting all locations");
        
        const string cacheKey = "locations_all";
        if (_cache.TryGetValue<List<LocationDto>>(cacheKey, out var cachedLocations))
        {
            _logger.LogDebug("Returning cached locations");
            return cachedLocations!;
        }
        
        // Note: This may need pagination if there are many locations
        var response = await _apiService.GetAsync<LocationsResponse>(BaseEndpoint, cancellationToken);
        var locations = ConvertToDtos(response?.Stores);
        
        _cache.Set(cacheKey, locations, _cacheExpiration);
        _logger.LogInformation("Retrieved and cached {Count} locations", locations.Count);
        
        return locations;
    }

    public async Task<List<LocationDto>> GetActiveLocationsAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting active locations");
        
        const string cacheKey = "locations_active";
        if (_cache.TryGetValue<List<LocationDto>>(cacheKey, out var cachedLocations))
        {
            _logger.LogDebug("Returning cached active locations");
            return cachedLocations!;
        }
        
        // Build filter for active locations
        var filter = new[]
        {
            new Dictionary<string, object>
            {
                ["property"] = "active",
                ["value"] = true
            }
        };
        
        var filterJson = JsonSerializer.Serialize(filter);
        var encodedFilter = HttpUtility.UrlEncode(filterJson);
        var endpoint = $"{BaseEndpoint}?filter={encodedFilter}";
        
        var response = await _apiService.GetAsync<LocationsResponse>(endpoint, cancellationToken);
        var locations = ConvertToDtos(response?.Stores);
        
        _cache.Set(cacheKey, locations, _cacheExpiration);
        _logger.LogInformation("Retrieved and cached {Count} active locations", locations.Count);
        
        return locations;
    }

    private List<LocationDto> ConvertToDtos(List<Location>? locations)
    {
        if (locations == null || !locations.Any())
            return new List<LocationDto>();
            
        return locations.Select(ConvertToDto).ToList();
    }

    private LocationDto ConvertToDto(Location location)
    {
        return new LocationDto
        {
            Id = location.Id,
            Name = location.Name,
            Identifier = location.Identifier,
            FacilityCode = location.FacilityCode,
            Active = location.Active,
            OccupiedBy = location.OccupiedBy,
            ClientCompany = location.EndUserCustomerRole?.DefaultAddress?.Company,
            LocationType = location.LocationType,
            SqFootage = location.SqFootage,
            
            // Address
            Address1 = location.StoreAddress?.Address1,
            Address2 = location.StoreAddress?.Address2,
            City = location.StoreAddress?.City,
            State = location.StoreAddress?.State,
            PostalCode = location.StoreAddress?.PostalCode,
            Country = location.StoreAddress?.Country,
            Phone = location.StoreAddress?.Phone,
            Email = location.StoreAddress?.Email,
            
            // Geographic
            Latitude = location.StoreAddress?.Latitude,
            Longitude = location.StoreAddress?.Longitude,
            Timezone = location.StoreAddress?.Timezone,
            
            // Dates
            OpenDate = location.OpenDate,
            CloseDate = location.CloseDate,
            CreatedAt = location.CreatedAt,
            UpdatedAt = location.UpdatedAt
        };
    }
}