using OrderAPI.Contracts;

namespace OrderAPI.Services;

public interface IUserApiClient
{
    Task<UserIdResponse> GetUserIdAsync(string provider, Guid providerOId, CancellationToken cancellationToken);
}