using CartAPI.Contracts;

namespace CartAPI.Services;

public interface IUserApiClient
{
    Task<UserIdResponse> GetUserIdAsync(string provider, Guid providerOId, CancellationToken cancellationToken);
}