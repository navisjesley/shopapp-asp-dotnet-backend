namespace ProductAPI.Contracts;

public sealed class ProductListItemDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
}