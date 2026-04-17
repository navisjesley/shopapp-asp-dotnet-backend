namespace CartAPI.Contracts;

public sealed class AddToCartRequest
{
    public int UserId { get; set; }
    public int ProductId { get; set; }
    public int QuantityToAdd { get; set; }
}