using System.Security.Claims;
using UserAPI.Contracts;
using UserAPI.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web.Resource;

namespace UserAPI.Controllers;

[ApiController]
[Route("user")]
[Authorize(Policy = "UserApiCaller")]
// [Authorize]
// [RequiredScope("access_as_user")]
public sealed class UserController(IUserBootstrapRepository repository) : ControllerBase
{
    [HttpPost("bootstrap")]
    [ProducesResponseType(typeof(UserBootstrapResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<UserBootstrapResponse>> Bootstrap([FromBody] UserBootstrapRequest request,
    CancellationToken cancellationToken)
    {
        // var provider = User.FindFirstValue("http://schemas.microsoft.com/identity/claims/identityprovider") ?? "Unknown";
        // var providerSubId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        // var providerOIdRaw = User.FindFirstValue("http://schemas.microsoft.com/identity/claims/objectidentifier");

        var provider = Request.Headers["X-User-Provider"].FirstOrDefault() ?? "Unknown";
        var providerSubId = Request.Headers["X-User-Sub"].FirstOrDefault();
        var providerOIdRaw = Request.Headers["X-User-Oid"].FirstOrDefault();

        if (string.IsNullOrWhiteSpace(providerSubId) || !Guid.TryParse(providerOIdRaw, out var providerOId))
        {
            return BadRequest("Missing external identifier in token.");
        }

        var response = await repository.UpsertFederatedUserOnLoginAsync(request, provider, providerSubId, providerOId, 
        cancellationToken);
        return Ok(response);
    }

    [HttpPost("id/by-provider-oid")]
    [ProducesResponseType(typeof(UserIdResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserIdResponse>> GetUserIdByProviderOid([FromBody] UserIdLookupRequest request,
    CancellationToken cancellationToken)
    {
        // var provider = User.FindFirstValue("http://schemas.microsoft.com/identity/claims/identityprovider") ?? "Unknown";
        // var providerOIdRaw = User.FindFirstValue("http://schemas.microsoft.com/identity/claims/objectidentifier");

        // if (!Guid.TryParse(providerOIdRaw, out var providerOId))
        // {
        //     return BadRequest("Missing oid in token.");
        // }

        if (string.IsNullOrWhiteSpace(request.Provider) || request.ProviderOId == Guid.Empty)
        {
            return BadRequest("Missing provider lookup values.");
        }

        try
        {
            var response = await repository.GetUserIdByProviderOidAsync(request.Provider, request.ProviderOId, cancellationToken);
            return Ok(response);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }
}