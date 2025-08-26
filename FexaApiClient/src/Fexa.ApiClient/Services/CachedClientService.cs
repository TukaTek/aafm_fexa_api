using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Fexa.ApiClient.Models;

namespace Fexa.ApiClient.Services;

public class CachedClientService : ICachedClientService
{
    private readonly IClientService _clientService;
    private readonly ILogger<CachedClientService> _logger;
    private readonly IMemoryCache _cache;
    
    private const string CACHE_KEY = "client_info_all";
    private const string CACHE_STATUS_KEY = "client_info_status";
    private readonly TimeSpan _cacheExpiration = TimeSpan.FromHours(4); // Cache for 4 hours since client data doesn't change often
    
    // Background refresh state
    private bool _isRefreshing = false;
    private DateTime _lastRefreshAttempt = DateTime.MinValue;
    private bool _lastRefreshSuccessful = true;

    public CachedClientService(
        IClientService clientService,
        ILogger<CachedClientService> logger,
        IMemoryCache cache)
    {
        _clientService = clientService ?? throw new ArgumentNullException(nameof(clientService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
    }

    public async Task<List<ClientInfo>> GetAllClientInfoAsync(CancellationToken cancellationToken = default)
    {
        // Check cache first
        if (_cache.TryGetValue(CACHE_KEY, out List<ClientInfo>? cachedClients))
        {
            _logger.LogDebug("Returning cached client info");
            return cachedClients ?? new List<ClientInfo>();
        }

        // Load from API if not cached
        return await LoadAndCacheClientsAsync(cancellationToken);
    }

    public async Task<List<ClientInfo>> GetActiveClientInfoAsync(CancellationToken cancellationToken = default)
    {
        var allClients = await GetAllClientInfoAsync(cancellationToken);
        return allClients.Where(c => c.Active).ToList();
    }

    public async Task<ClientInfo?> GetClientInfoByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var allClients = await GetAllClientInfoAsync(cancellationToken);
        return allClients.FirstOrDefault(c => c.Id == id);
    }

    public async Task<ClientInfo?> GetClientInfoByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(name))
            return null;
            
        var allClients = await GetAllClientInfoAsync(cancellationToken);
        
        // Check exact match first (case insensitive)
        var client = allClients.FirstOrDefault(c => 
            string.Equals(c.Name, name, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(c.Dba, name, StringComparison.OrdinalIgnoreCase));
            
        return client;
    }

    public async Task<ClientInfo?> GetClientInfoByIvrIdAsync(string ivrId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(ivrId))
            return null;
            
        var allClients = await GetAllClientInfoAsync(cancellationToken);
        return allClients.FirstOrDefault(c => 
            string.Equals(c.IvrId, ivrId, StringComparison.OrdinalIgnoreCase));
    }

    public async Task<List<ClientInfo>> SearchClientInfoAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return new List<ClientInfo>();
            
        var allClients = await GetAllClientInfoAsync(cancellationToken);
        var lowerSearch = searchTerm.ToLower();
        
        return allClients
            .Where(c => 
                c.Name.ToLower().Contains(lowerSearch) ||
                (c.Dba != null && c.Dba.ToLower().Contains(lowerSearch)) ||
                (c.IvrId != null && c.IvrId.ToLower().Contains(lowerSearch)))
            .OrderBy(c => c.Name)
            .ToList();
    }

    public async Task<List<ClientInfo>> RefreshCacheAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Forcing cache refresh");
        _cache.Remove(CACHE_KEY);
        _cache.Remove(CACHE_STATUS_KEY);
        return await LoadAndCacheClientsAsync(cancellationToken);
    }

    public async Task RefreshCacheInBackgroundAsync()
    {
        if (_isRefreshing)
        {
            _logger.LogWarning("Cache refresh already in progress, skipping");
            return;
        }

        _ = Task.Run(async () =>
        {
            try
            {
                _isRefreshing = true;
                _lastRefreshAttempt = DateTime.UtcNow;
                _logger.LogInformation("Starting background cache refresh for client info...");

                var clientInfoList = await LoadClientsFromApiAsync();
                
                if (clientInfoList.Any())
                {
                    // Update cache atomically
                    _cache.Set(CACHE_KEY, clientInfoList, _cacheExpiration);
                    _lastRefreshSuccessful = true;
                    
                    // Update status
                    var status = new ClientCacheStatus
                    {
                        LastRefreshed = DateTime.UtcNow,
                        ItemCount = clientInfoList.Count,
                        ActiveCount = clientInfoList.Count(c => c.Active),
                        LastRefreshAttempt = _lastRefreshAttempt,
                        LastRefreshSuccessful = true,
                        IsRefreshing = false
                    };
                    _cache.Set(CACHE_STATUS_KEY, status, _cacheExpiration);
                    
                    _logger.LogInformation("Background cache refresh completed successfully with {Count} clients ({Active} active)", 
                        clientInfoList.Count, status.ActiveCount);
                }
                else
                {
                    _lastRefreshSuccessful = false;
                    _logger.LogWarning("Background cache refresh returned no data");
                }
            }
            catch (Exception ex)
            {
                _lastRefreshSuccessful = false;
                _logger.LogError(ex, "Background cache refresh failed");
            }
            finally
            {
                _isRefreshing = false;
            }
        });
    }

    public async Task<ClientCacheStatus> GetCacheStatusAsync()
    {
        // Try to get cached status first
        if (_cache.TryGetValue(CACHE_STATUS_KEY, out ClientCacheStatus? cachedStatus) && cachedStatus != null)
        {
            cachedStatus.IsRefreshing = _isRefreshing;
            return cachedStatus;
        }
        
        // Build status from current state
        var clients = await GetAllClientInfoAsync();
        return new ClientCacheStatus
        {
            LastRefreshed = DateTime.UtcNow.AddMinutes(-30), // Estimate if not cached
            IsRefreshing = _isRefreshing,
            ItemCount = clients.Count,
            ActiveCount = clients.Count(c => c.Active),
            LastRefreshAttempt = _lastRefreshAttempt,
            LastRefreshSuccessful = _lastRefreshSuccessful
        };
    }

    private async Task<List<ClientInfo>> LoadAndCacheClientsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Loading all clients from API for caching...");
            
            var clientInfoList = await LoadClientsFromApiAsync(cancellationToken);
            
            // Cache the results
            _cache.Set(CACHE_KEY, clientInfoList, _cacheExpiration);
            
            // Update status
            var status = new ClientCacheStatus
            {
                LastRefreshed = DateTime.UtcNow,
                ItemCount = clientInfoList.Count,
                ActiveCount = clientInfoList.Count(c => c.Active),
                LastRefreshAttempt = DateTime.UtcNow,
                LastRefreshSuccessful = true,
                IsRefreshing = false
            };
            _cache.Set(CACHE_STATUS_KEY, status, _cacheExpiration);
            
            _logger.LogInformation("Successfully cached {Count} clients ({Active} active)", 
                clientInfoList.Count, status.ActiveCount);
            
            return clientInfoList;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading and caching client data");
            throw;
        }
    }

    private async Task<List<ClientInfo>> LoadClientsFromApiAsync(CancellationToken cancellationToken = default)
    {
        var clientInfoList = new List<ClientInfo>();
        
        // Use the existing client service which already handles pagination
        var allClients = await _clientService.GetAllClientsAsync(null, 100, cancellationToken); // Max 100 pages
        
        // Transform to simplified ClientInfo
        foreach (var client in allClients)
        {
            var clientInfo = new ClientInfo
            {
                Id = client.Id,
                Name = GetClientName(client),
                Dba = GetClientDba(client),
                Active = client.Active,
                IvrId = client.IvrId
            };
            
            clientInfoList.Add(clientInfo);
        }
        
        return clientInfoList;
    }

    private string GetClientName(Client client)
    {
        // Try to get company name from addresses
        var company = client.DefaultGeneralAddress?.Company ?? 
                     client.DefaultBillingAddress?.Company ??
                     client.DefaultGeneralAddress?.Dba ??
                     client.DefaultBillingAddress?.Dba;
                     
        if (!string.IsNullOrWhiteSpace(company))
            return company;
            
        // Fallback to ID if no name found
        return $"Client {client.Id}";
    }

    private string? GetClientDba(Client client)
    {
        var dba = client.DefaultGeneralAddress?.Dba ?? 
                  client.DefaultBillingAddress?.Dba;
                  
        // Only return DBA if it's different from the company name
        var company = client.DefaultGeneralAddress?.Company ?? 
                     client.DefaultBillingAddress?.Company;
                     
        if (!string.IsNullOrWhiteSpace(dba) && 
            !string.Equals(dba, company, StringComparison.OrdinalIgnoreCase))
        {
            return dba;
        }
        
        return null;
    }
}