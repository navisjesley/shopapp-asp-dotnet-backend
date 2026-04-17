using CartAPI.Data;
using CartAPI.Contracts;
using CartAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web.Resource;
using Microsoft.AspNetCore.Authorization;

namespace CartAPI.Controllers;

[ApiController]
[Route("cart")]
[Authorize(Policy = "CartApiCaller")]
// [Authorize]
// [RequiredScope("access_as_user")]
public sealed class CartController(ICartRepository cartRepository, IUserApiClient userApiClient) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> AddItemToCart([FromBody] AddToCartRequest request, CancellationToken cancellationToken)
    {
        if (request.QuantityToAdd <= 0)
        {
            ModelState.AddModelError(nameof(request.QuantityToAdd), "QuantityToAdd must be greater than zero.");
            return ValidationProblem(ModelState);
        }

        // await cartRepository.AddItemToCartAsync(request, cancellationToken);

        var provider = Request.Headers["X-User-Provider"].FirstOrDefault() ?? "Unknown";
        var providerOIdRaw = Request.Headers["X-User-Oid"].FirstOrDefault();

        if (!Guid.TryParse(providerOIdRaw, out var providerOId))
        {
            return BadRequest("Missing oid in forwarded headers.");
        }

        var userIdResponse = await userApiClient.GetUserIdAsync(provider, providerOId, cancellationToken);
        await cartRepository.AddItemToCartAsync(request, userIdResponse.UserId, cancellationToken);

        return NoContent();
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<CartItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IReadOnlyList<CartItemDto>>> GetCart(CancellationToken cancellationToken)
    {
        var provider = Request.Headers["X-User-Provider"].FirstOrDefault() ?? "Unknown";
        var providerOIdRaw = Request.Headers["X-User-Oid"].FirstOrDefault();

        if (!Guid.TryParse(providerOIdRaw, out var providerOId))
        {
            return BadRequest("Missing oid in forwarded headers.");
        }

        var userIdResponse = await userApiClient.GetUserIdAsync(provider, providerOId, cancellationToken);
        var cartItems = await cartRepository.GetCartByUserIdAsync(userIdResponse.UserId, cancellationToken);

        return Ok(cartItems);
    }

    [HttpPost("delete-item")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DeleteItemFromCart([FromBody] DeleteCartItemRequest request, CancellationToken cancellationToken)
    {
        var provider = Request.Headers["X-User-Provider"].FirstOrDefault() ?? "Unknown";
        var providerOIdRaw = Request.Headers["X-User-Oid"].FirstOrDefault();

        if (!Guid.TryParse(providerOIdRaw, out var providerOId))
        {
            return BadRequest("Missing oid in forwarded headers.");
        }

        var userIdResponse = await userApiClient.GetUserIdAsync(provider, providerOId, cancellationToken);
        await cartRepository.DeleteItemFromCartAsync(request, userIdResponse.UserId, cancellationToken);
        return NoContent();
    }
}