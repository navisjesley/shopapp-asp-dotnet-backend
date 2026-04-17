namespace OrderAPI.Contracts;

public sealed class PlaceOrderRequest
{
    public int CartId { get; set; }
    public string DeliveryAddress { get; set; } = string.Empty;
    public IReadOnlyList<PlaceOrderItemRequest> Items { get; set; } = Array.Empty<PlaceOrderItemRequest>();
}