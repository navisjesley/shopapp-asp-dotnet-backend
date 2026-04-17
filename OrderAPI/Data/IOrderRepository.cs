using OrderAPI.Contracts;

namespace OrderAPI.Data;

public interface IOrderRepository
{
    Task<PlaceOrderResponse> PlaceOrderAsync(int userId, PlaceOrderRequest request, CancellationToken cancellationToken);
}