using OrderAPI.Data;
using OrderAPI.Services;
using OrderAPI.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web.Resource;
using Microsoft.AspNetCore.Authorization;

namespace OrderAPI.Controllers;

[ApiController]
[Route("orders")]
[Authorize(Policy = "OrderApiCaller")]
// [Authorize]
// [RequiredScope("access_as_user")]
public sealed class OrdersController(IOrderRepository orderRepository, IUserApiClient userApiClient) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(PlaceOrderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PlaceOrderResponse>> PlaceOrder([FromBody] PlaceOrderRequest request,
    CancellationToken cancellationToken)
    {
        if (request is null || request.CartId <= 0 || string.IsNullOrWhiteSpace(request.DeliveryAddress) || 
        request.Items is null || request.Items.Count == 0)
        {
            return BadRequest("Request body is problematic.");
        }

        var provider = Request.Headers["X-User-Provider"].FirstOrDefault() ?? "Unknown";
        var providerOIdRaw = Request.Headers["X-User-Oid"].FirstOrDefault();

        if (!Guid.TryParse(providerOIdRaw, out var providerOId))
        {
            return BadRequest("Missing oid in forwarded headers.");
        }

        var userIdResponse = await userApiClient.GetUserIdAsync(provider, providerOId, cancellationToken);
        var response = await orderRepository.PlaceOrderAsync(userIdResponse.UserId, request, cancellationToken);

        return Ok(response);
    }
}