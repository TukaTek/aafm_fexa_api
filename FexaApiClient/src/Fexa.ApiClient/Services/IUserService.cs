using Fexa.ApiClient.Models;

namespace Fexa.ApiClient.Services;

public interface IUserService
{
    Task<User> GetUserAsync(string userId, CancellationToken cancellationToken = default);
    Task<PagedResponse<User>> GetUsersAsync(QueryParameters? parameters = null, CancellationToken cancellationToken = default);
    Task<User> CreateUserAsync(CreateUserRequest request, CancellationToken cancellationToken = default);
    Task<User> UpdateUserAsync(string userId, UpdateUserRequest request, CancellationToken cancellationToken = default);
    Task DeleteUserAsync(string userId, CancellationToken cancellationToken = default);
}