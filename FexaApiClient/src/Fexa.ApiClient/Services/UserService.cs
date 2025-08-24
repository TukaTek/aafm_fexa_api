using Microsoft.Extensions.Logging;
using Fexa.ApiClient.Models;

namespace Fexa.ApiClient.Services;

public class UserService : IUserService
{
    private readonly IFexaApiService _apiService;
    private readonly ILogger<UserService> _logger;
    private const string UsersEndpoint = "/api/users";
    
    public UserService(IFexaApiService apiService, ILogger<UserService> logger)
    {
        _apiService = apiService ?? throw new ArgumentNullException(nameof(apiService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
    
    public async Task<User> GetUserAsync(string userId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("User ID cannot be null or empty", nameof(userId));
            
        _logger.LogDebug("Getting user with ID: {UserId}", userId);
        
        var response = await _apiService.GetAsync<BaseResponse<User>>(
            $"{UsersEndpoint}/{userId}", 
            cancellationToken);
            
        return response.Data ?? throw new InvalidOperationException("User not found");
    }
    
    public async Task<PagedResponse<User>> GetUsersAsync(
        QueryParameters? parameters = null, 
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting users with parameters: {@Parameters}", parameters);
        
        var queryString = parameters?.ToDictionary() ?? new Dictionary<string, string>();
        var query = string.Join("&", queryString.Select(kvp => $"{kvp.Key}={Uri.EscapeDataString(kvp.Value)}"));
        var endpoint = string.IsNullOrEmpty(query) ? UsersEndpoint : $"{UsersEndpoint}?{query}";
        
        return await _apiService.GetAsync<PagedResponse<User>>(endpoint, cancellationToken);
    }
    
    public async Task<User> CreateUserAsync(CreateUserRequest request, CancellationToken cancellationToken = default)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));
            
        _logger.LogDebug("Creating user with email: {Email}", request.Email);
        
        var response = await _apiService.PostAsync<BaseResponse<User>>(
            UsersEndpoint, 
            request, 
            cancellationToken);
            
        return response.Data ?? throw new InvalidOperationException("Failed to create user");
    }
    
    public async Task<User> UpdateUserAsync(
        string userId, 
        UpdateUserRequest request, 
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("User ID cannot be null or empty", nameof(userId));
            
        if (request == null)
            throw new ArgumentNullException(nameof(request));
            
        _logger.LogDebug("Updating user with ID: {UserId}", userId);
        
        var response = await _apiService.PatchAsync<BaseResponse<User>>(
            $"{UsersEndpoint}/{userId}", 
            request, 
            cancellationToken);
            
        return response.Data ?? throw new InvalidOperationException("Failed to update user");
    }
    
    public async Task DeleteUserAsync(string userId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("User ID cannot be null or empty", nameof(userId));
            
        _logger.LogDebug("Deleting user with ID: {UserId}", userId);
        
        await _apiService.DeleteAsync<BaseResponse<object>>(
            $"{UsersEndpoint}/{userId}", 
            cancellationToken);
    }
}