namespace OrderAPI.Contracts;

public sealed class PlaceOrderItemRequest
{
    public int CartItemId { get; set; }
    public int ProductId { get; set; }
    public int QuantityInCart { get; set; }
}