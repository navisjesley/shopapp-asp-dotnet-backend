using CartAPI.Contracts;

namespace CartAPI.Data;

public interface ICartRepository
{
    Task AddItemToCartAsync(AddToCartRequest request, int userId, CancellationToken cancellationToken);
    Task<IReadOnlyList<CartItemDto>> GetCartByUserIdAsync(int userId, CancellationToken cancellationToken);
    Task DeleteItemFromCartAsync(DeleteCartItemRequest request, int userId, CancellationToken cancellationToken);
}