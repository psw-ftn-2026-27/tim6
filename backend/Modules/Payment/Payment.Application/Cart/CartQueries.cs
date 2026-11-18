using Payment.Application.Carts;
using Payment.Application.Wallets;

namespace Payment.Application.Cart;

public sealed class CartQueries
{
    private readonly ICartReadRepository _cartReadRepository;
    private readonly ICartRepository _cartRepository;
    private readonly IWalletRepository _walletRepository;

    public CartQueries(
        ICartReadRepository cartReadRepository,
        ICartRepository cartRepository,
        IWalletRepository walletRepository)
    {
        _cartReadRepository = cartReadRepository;
        _cartRepository = cartRepository;
        _walletRepository = walletRepository;
    }

    public async Task<CartDto> GetAsync(Guid userId)
    {
        var wallet = await _walletRepository.GetByUserIdAsync(userId);
        var walletBalance = wallet is null ? 0 : wallet.Balance();

        var cart = await _cartRepository.GetByUserIdAsync(userId);
        if (cart is null)
        {
            return new CartDto([], 0, walletBalance);
        }

        var items = await _cartReadRepository.GetItemsAsync(cart.Id);
        return new CartDto(items, cart.TotalPrice(), walletBalance);
    }
}
