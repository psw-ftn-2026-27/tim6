using Payment.Application.Cart;

namespace Payment.Application.Carts;

public interface ICartReadRepository
{
    Task<List<CartItemDto>> GetItemsAsync(Guid cartId);
}
