using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Fexa.ApiClient.Configuration;
using Fexa.ApiClient.Exceptions;

namespace Fexa.ApiClient.Services;

public class FexaApiService : IFexaApiService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<FexaApiService> _logger;
    private readonly FexaApiOptions _options;
    private readonly JsonSerializerOptions _jsonOptions;
    
    public FexaApiService(
        HttpClient httpClient,
        ILogger<FexaApiService> logger,
        IOptions<FexaApiOptions> options)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };
    }
    
    public async Task<T> GetAsync<T>(string endpoint, CancellationToken cancellationToken = default)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
        return await SendRequestAsync<T>(request, cancellationToken);
    }
    
    public async Task<T> PostAsync<T>(string endpoint, object? payload = null, CancellationToken cancellationToken = default)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, endpoint);
        if (payload != null)
        {
            request.Content = CreateJsonContent(payload);
        }
        return await SendRequestAsync<T>(request, cancellationToken);
    }
    
    public async Task<T> PutAsync<T>(string endpoint, object? payload = null, CancellationToken cancellationToken = default)
    {
        var request = new HttpRequestMessage(HttpMethod.Put, endpoint);
        if (payload != null)
        {
            request.Content = CreateJsonContent(payload);
        }
        return await SendRequestAsync<T>(request, cancellationToken);
    }
    
    public async Task<T> PatchAsync<T>(string endpoint, object? payload = null, CancellationToken cancellationToken = default)
    {
        var request = new HttpRequestMessage(HttpMethod.Patch, endpoint);
        if (payload != null)
        {
            request.Content = CreateJsonContent(payload);
        }
        return await SendRequestAsync<T>(request, cancellationToken);
    }
    
    public async Task<T> DeleteAsync<T>(string endpoint, CancellationToken cancellationToken = default)
    {
        var request = new HttpRequestMessage(HttpMethod.Delete, endpoint);
        return await SendRequestAsync<T>(request, cancellationToken);
    }
    
    public async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken = default)
    {
        return await _httpClient.SendAsync(request, cancellationToken);
    }
    
    private async Task<T> SendRequestAsync<T>(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogDebug("Sending {Method} request to {Uri}", request.Method, request.RequestUri);
            
            var response = await _httpClient.SendAsync(request, cancellationToken);
            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            
            if (response.IsSuccessStatusCode)
            {
                _logger.LogDebug("Request successful with status {StatusCode}", response.StatusCode);
                // Only log first 500 chars of response to avoid overwhelming output
                var truncatedContent = responseContent.Length > 500 
                    ? responseContent.Substring(0, 500) + "..." 
                    : responseContent;
                _logger.LogDebug("Raw response content (truncated): {Content}", truncatedContent);
                
                if (string.IsNullOrWhiteSpace(responseContent))
                {
                    return default(T)!;
                }
                
                try
                {
                    return JsonSerializer.Deserialize<T>(responseContent, _jsonOptions)!;
                }
                catch (JsonException ex)
                {
                    _logger.LogError(ex, "Failed to deserialize response content");
                    throw new FexaApiException("Failed to deserialize API response", ex);
                }
            }
            
            _logger.LogWarning("Request failed with status {StatusCode}: {Content}", 
                response.StatusCode, responseContent);
            
            await HandleErrorResponse(response, responseContent);
            
            // This line should never be reached due to HandleErrorResponse throwing
            throw new FexaApiException($"Unexpected error occurred. Status: {response.StatusCode}");
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP request exception occurred");
            throw new FexaApiException("Network error occurred while calling API", ex);
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "Request timeout or cancellation");
            throw new FexaApiException("Request timeout or was cancelled", ex);
        }
        catch (FexaApiException)
        {
            // Re-throw our custom exceptions
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred");
            throw new FexaApiException("An unexpected error occurred", ex);
        }
    }
    
    private Task HandleErrorResponse(HttpResponseMessage response, string responseContent)
    {
        var requestId = response.Headers.TryGetValues("X-Request-Id", out var values) 
            ? values.FirstOrDefault() 
            : null;
            
        switch (response.StatusCode)
        {
            case HttpStatusCode.Unauthorized:
                throw new FexaAuthenticationException(
                    "Authentication failed. Please check your API key.", 
                    response.StatusCode, 
                    responseContent);
                    
            case HttpStatusCode.Forbidden:
                throw new FexaAuthenticationException(
                    "Access forbidden. You don't have permission to access this resource.", 
                    response.StatusCode, 
                    responseContent);
                    
            case HttpStatusCode.TooManyRequests:
                var retryAfter = response.Headers.RetryAfter?.Delta?.TotalSeconds;
                throw new FexaRateLimitException(
                    "Rate limit exceeded. Please retry later.", 
                    (int?)retryAfter);
                    
            case HttpStatusCode.BadRequest:
                var validationErrors = TryParseValidationErrors(responseContent);
                throw new FexaValidationException(
                    "Validation failed. Please check your request data.", 
                    validationErrors);
                    
            case HttpStatusCode.NotFound:
                throw new FexaApiException(
                    "Resource not found.", 
                    response.StatusCode, 
                    responseContent, 
                    requestId);
                    
            case HttpStatusCode.InternalServerError:
            case HttpStatusCode.BadGateway:
            case HttpStatusCode.ServiceUnavailable:
            case HttpStatusCode.GatewayTimeout:
                throw new FexaApiException(
                    "Server error occurred. Please try again later.", 
                    response.StatusCode, 
                    responseContent, 
                    requestId);
                    
            default:
                throw new FexaApiException(
                    $"API request failed with status {response.StatusCode}", 
                    response.StatusCode, 
                    responseContent, 
                    requestId);
        }
        
        return Task.CompletedTask; // This line will never be reached
    }
    
    private Dictionary<string, string[]>? TryParseValidationErrors(string responseContent)
    {
        try
        {
            var errorResponse = JsonSerializer.Deserialize<Dictionary<string, object>>(responseContent, _jsonOptions);
            if (errorResponse?.ContainsKey("errors") == true)
            {
                // Try to parse validation errors from the response
                // This is a simplified version - adjust based on actual API response format
                return JsonSerializer.Deserialize<Dictionary<string, string[]>>(
                    errorResponse["errors"].ToString()!, 
                    _jsonOptions);
            }
        }
        catch
        {
            // Ignore parsing errors
        }
        
        return null;
    }
    
    private StringContent CreateJsonContent(object payload)
    {
        var json = JsonSerializer.Serialize(payload, _jsonOptions);
        return new StringContent(json, Encoding.UTF8, "application/json");
    }
}