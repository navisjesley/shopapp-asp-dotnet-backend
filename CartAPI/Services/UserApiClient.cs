using System.Net.Http.Headers;
using System.Net.Http.Json;
using CartAPI.Contracts;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Web;
using Microsoft.Identity.Client;

namespace CartAPI.Services;

public sealed class UserApiClient(HttpClient httpClient, IOptions<UserApiOptions> options) : IUserApiClient
{
    private readonly UserApiOptions _options = options.Value;

    public async Task<UserIdResponse> GetUserIdAsync(string provider, Guid providerOId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(provider))
        {
            throw new ArgumentException("Provider is required.", nameof(provider));
        }

        if (providerOId == Guid.Empty)
        {
            throw new ArgumentException("ProviderOId is required.", nameof(providerOId));
        }

        var authority = $"https://login.microsoftonline.com/{_options.TenantId}";

        var app = ConfidentialClientApplicationBuilder
            .Create(_options.ClientId)
            .WithClientSecret(_options.ClientSecret)
            .WithAuthority(authority)
            .Build();

        var result = await app
            .AcquireTokenForClient(new[] { $"{_options.Resource}/.default" })
            .ExecuteAsync(cancellationToken);

        using var request = new HttpRequestMessage(HttpMethod.Post, "user/id/by-provider-oid");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", result.AccessToken);
        request.Content = JsonContent.Create(new UserIdLookupRequest
        {
            Provider = provider,
            ProviderOId = providerOId
        });

        using var response = await httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var userIdResponse = await response.Content.ReadFromJsonAsync<UserIdResponse>(
            cancellationToken: cancellationToken);

        if (userIdResponse is null)
        {
            throw new InvalidOperationException("User API returned an empty response.");
        }

        return userIdResponse;
    }
}





// public sealed class UserApiClient(HttpClient httpClient, ITokenAcquisition tokenAcquisition, 
// IConfiguration configuration) : IUserApiClient
// {
//     public async Task<UserIdResponse> GetUserIdAsync(CancellationToken cancellationToken)
//     {
//         var scopes = configuration.GetSection("UserApi:Scopes").Get<string[]>() ?? 
//         throw new InvalidOperationException("User API scopes are not configured.");

//         var accessToken = await tokenAcquisition.GetAccessTokenForUserAsync(scopes);

//         using var request = new HttpRequestMessage(HttpMethod.Get, "user/id");
//         request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

//         using var response = await httpClient.SendAsync(request, cancellationToken);
//         response.EnsureSuccessStatusCode();

//         var userIdResponse = await response.Content.ReadFromJsonAsync<UserIdResponse>(cancellationToken: cancellationToken);
//         if (userIdResponse is null)
//         {
//             throw new InvalidOperationException("User API returned an empty response.");
//         }

//         return userIdResponse;
//     }
// }