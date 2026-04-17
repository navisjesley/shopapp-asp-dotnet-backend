using UserAPI.Contracts;

namespace UserAPI.Data;

public interface IUserBootstrapRepository
{
    Task<UserBootstrapResponse> UpsertFederatedUserOnLoginAsync(UserBootstrapRequest request, string provider, 
    string providerSubId, Guid providerOId, CancellationToken cancellationToken);
    Task<UserIdResponse> GetUserIdByProviderOidAsync(string provider, Guid providerOId, CancellationToken cancellationToken);
}