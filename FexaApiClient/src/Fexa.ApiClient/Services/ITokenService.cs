using Fexa.ApiClient.Models;

namespace Fexa.ApiClient.Services;

public interface ITokenService
{
    Task<string> GetAccessTokenAsync(CancellationToken cancellationToken = default);
    Task<TokenResponse> RefreshTokenAsync(CancellationToken cancellationToken = default);
}