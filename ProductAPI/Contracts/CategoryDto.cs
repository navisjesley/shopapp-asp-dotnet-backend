namespace ProductAPI.Contracts;

public sealed class CategoryDto
{
    public int ProductCategoryId { get; set; }
    public string ProductCategory { get; set; } = string.Empty;
}