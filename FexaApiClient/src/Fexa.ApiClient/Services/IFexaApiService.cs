namespace Fexa.ApiClient.Services;

public interface IFexaApiService
{
    Task<T> GetAsync<T>(string endpoint, CancellationToken cancellationToken = default);
    Task<T> PostAsync<T>(string endpoint, object? payload = null, CancellationToken cancellationToken = default);
    Task<T> PutAsync<T>(string endpoint, object? payload = null, CancellationToken cancellationToken = default);
    Task<T> PatchAsync<T>(string endpoint, object? payload = null, CancellationToken cancellationToken = default);
    Task<T> DeleteAsync<T>(string endpoint, CancellationToken cancellationToken = default);
    Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken = default);
}