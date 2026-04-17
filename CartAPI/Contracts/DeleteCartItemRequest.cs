namespace CartAPI.Contracts;

public sealed class DeleteCartItemRequest
{
    public int CartItemId { get; set; }
    public int CartId { get; set; }
    public int ProductId { get; set; }
}