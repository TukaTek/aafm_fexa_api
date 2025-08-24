using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Fexa.ApiClient.Configuration;
using Fexa.ApiClient.Models;
using Fexa.ApiClient.Exceptions;

namespace Fexa.ApiClient.Services;

public class TokenService : ITokenService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<TokenService> _logger;
    private readonly FexaApiOptions _options;
    private readonly SemaphoreSlim _tokenSemaphore = new(1, 1);
    private TokenResponse? _currentToken;
    
    public TokenService(HttpClient httpClient, ILogger<TokenService> logger, IOptions<FexaApiOptions> options)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
    }
    
    public async Task<string> GetAccessTokenAsync(CancellationToken cancellationToken = default)
    {
        await _tokenSemaphore.WaitAsync(cancellationToken);
        try
        {
            // Check if we have a valid token
            if (_currentToken != null && _currentToken.ExpiresAt > DateTime.UtcNow.AddSeconds(_options.TokenRefreshBufferSeconds))
            {
                return _currentToken.AccessToken;
            }
            
            // Need to get a new token
            _logger.LogDebug("Acquiring new access token");
            _currentToken = await AcquireTokenAsync(cancellationToken);
            
            return _currentToken.AccessToken;
        }
        finally
        {
            _tokenSemaphore.Release();
        }
    }
    
    public async Task<TokenResponse> RefreshTokenAsync(CancellationToken cancellationToken = default)
    {
        await _tokenSemaphore.WaitAsync(cancellationToken);
        try
        {
            _logger.LogDebug("Forcing token refresh");
            _currentToken = await AcquireTokenAsync(cancellationToken);
            return _currentToken;
        }
        finally
        {
            _tokenSemaphore.Release();
        }
    }
    
    private async Task<TokenResponse> AcquireTokenAsync(CancellationToken cancellationToken)
    {
        try
        {
            // Ensure token endpoint starts with /
            var tokenEndpoint = _options.TokenEndpoint.StartsWith("/") 
                ? _options.TokenEndpoint 
                : "/" + _options.TokenEndpoint;
            
            // Add grant_type as query parameter
            tokenEndpoint = $"{tokenEndpoint}?grant_type=client_credentials";
            
            // Create JSON body with client credentials
            var requestBody = new
            {
                client_id = _options.ClientId,
                client_secret = _options.ClientSecret
            };
            
            var jsonBody = JsonSerializer.Serialize(requestBody);
            
            var jsonContent = new StringContent(
                jsonBody,
                Encoding.UTF8,
                "application/json");
            
            // Create request message
            var request = new HttpRequestMessage(HttpMethod.Post, tokenEndpoint)
            {
                Content = jsonContent
            };
                
            var response = await _httpClient.SendAsync(request, cancellationToken);
            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Token acquisition failed with status {StatusCode}: {Content}", 
                    response.StatusCode, responseContent);
                    
                throw new FexaAuthenticationException(
                    $"Failed to acquire access token. Status: {response.StatusCode}", 
                    response.StatusCode, 
                    responseContent);
            }
            
            var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(responseContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            
            if (tokenResponse == null || string.IsNullOrWhiteSpace(tokenResponse.AccessToken))
            {
                throw new FexaAuthenticationException("Invalid token response received");
            }
            
            // Calculate expiration time
            tokenResponse.ExpiresAt = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn);
            
            _logger.LogDebug("Successfully acquired access token, expires at {ExpiresAt}", tokenResponse.ExpiresAt);
            
            return tokenResponse;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Network error while acquiring token");
            throw new FexaApiException("Network error occurred while acquiring access token", ex);
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "Token request timeout");
            throw new FexaApiException("Token request timed out", ex);
        }
        catch (FexaAuthenticationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while acquiring token");
            throw new FexaApiException("Unexpected error occurred while acquiring access token", ex);
        }
    }
}