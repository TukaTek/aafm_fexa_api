using Fexa.ApiClient.Services;

namespace Fexa.ApiClient.Http;

public class FexaAuthenticationHandler : DelegatingHandler
{
    private readonly ITokenService _tokenService;
    
    public FexaAuthenticationHandler(ITokenService tokenService)
    {
        _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
    }
    
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, 
        CancellationToken cancellationToken)
    {
        // Skip authentication for token endpoint
        if (request.RequestUri?.AbsolutePath.Contains("/oauth/token", StringComparison.OrdinalIgnoreCase) == true)
        {
            return await base.SendAsync(request, cancellationToken);
        }
        
        // Get access token and add to request
        var accessToken = await _tokenService.GetAccessTokenAsync(cancellationToken);
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        
        // Add Accept header to specify JSON format (required by Fexa API)
        request.Headers.Accept.Clear();
        request.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
        
        return await base.SendAsync(request, cancellationToken);
    }
}