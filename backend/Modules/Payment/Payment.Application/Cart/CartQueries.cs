using Payment.Application.Carts;

namespace Payment.Application.Cart;

public sealed class CartQueries
{
    private readonly ICartReadRepository _cartReadRepository;
    private readonly ICartRepository _cartRepository;

    public CartQueries(ICartReadRepository cartReadRepository, ICartRepository cartRepository)
    {
        _cartReadRepository = cartReadRepository;
        _cartRepository = cartRepository;
    }

    public async Task<CartDto> GetAsync(Guid userId)
    {
        var cart = await _cartRepository.GetByUserIdAsync(userId);
        if (cart is null)
        {
            return new CartDto([], 0);
        }

        var items = await _cartReadRepository.GetItemsAsync(cart.Id);
        return new CartDto(items, cart.TotalPrice());
    }
}
