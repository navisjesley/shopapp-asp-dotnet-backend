namespace CartAPI.Contracts;

public sealed class CartItemDto
{
    public int CartItemId { get; set; }
    public int CartId { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int QuantityInCart { get; set; }
}