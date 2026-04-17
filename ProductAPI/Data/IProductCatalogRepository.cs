using ProductAPI.Contracts;

namespace ProductAPI.Data;

public interface IProductCatalogRepository
{
    Task<IReadOnlyList<CategoryDto>> GetCategoriesAsync(CancellationToken cancellationToken);
    Task<IReadOnlyList<ProductListItemDto>> GetProductsByCategoryAsync(int categoryId, CancellationToken cancellationToken);
    Task<ProductDetailDto?> GetProductByIdAsync(int productId, CancellationToken cancellationToken);
}