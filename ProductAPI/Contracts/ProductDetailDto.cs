namespace ProductAPI.Contracts;

public sealed class ProductDetailDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set;}
    public string ProductImageUrl { get; set; } = string.Empty;
}