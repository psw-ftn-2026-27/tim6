using Payment.Application.Carts;
using Payment.Application.Coupons;
using Payment.Application.GiftCards;
using Payment.Application.Products;
using Payment.Application.Wallets;
using Payment.Domain.Carts;
using Payment.Domain.Products;
using Shared.Domain.Exceptions;

namespace Payment.Application.Cart;

public sealed class CartQueries
{
    private readonly ICartRepository _cartRepository;
    private readonly IWalletRepository _walletRepository;
    private readonly IProductRepository _productRepository;
    private readonly ICouponRepository _couponRepository;
    private readonly IGiftCardRepository _giftCardRepository;
    private readonly CartCheckout _cartCheckout;

    public CartQueries(
        ICartRepository cartRepository,
        IWalletRepository walletRepository,
        IProductRepository productRepository,
        ICouponRepository couponRepository,
        IGiftCardRepository giftCardRepository,
        CartCheckout cartCheckout)
    {
        _cartRepository = cartRepository;
        _walletRepository = walletRepository;
        _productRepository = productRepository;
        _couponRepository = couponRepository;
        _giftCardRepository = giftCardRepository;
        _cartCheckout = cartCheckout;
    }

    public async Task<CartDto> GetAsync(Guid userId, string? couponCode = null, Guid? giftCardId = null)
    {
        var wallet = await _walletRepository.GetByUserIdAsync(userId);
        var walletBalance = wallet is null ? 0 : wallet.Balance();

        var cart = await _cartRepository.GetByUserIdAsync(userId);
        if (cart is null)
        {
            return new CartDto([], 0, walletBalance, 0, 0, 0, 0);
        }

        var products = await _productRepository.GetByEntityIdsAsync(cart.Items.Select(item => item.EntityId).ToList());
        var coupon = await GetCouponAsync(couponCode);
        var giftCard = await GetGiftCardAsync(giftCardId);
        var cartPrice = _cartCheckout.CalculatePrice(cart, products, coupon, giftCard, DateTime.UtcNow);
        return ToDto(cart, products, walletBalance, cartPrice);
    }

    private static CartDto ToDto(
        Payment.Domain.Carts.Cart cart,
        IReadOnlyList<Product> products,
        int walletBalance,
        CartPrice cartPrice)
    {
        var items = cartPrice.Items.Select(item => new CartItemDto(
            item.EntityId,
            item.Name,
            item.OriginalPrice,
            products.Any(product => product.EntityId == item.EntityId && product.IsInOffer),
            item.CouponDiscount,
            item.PriceAfterCoupon)).ToList();

        return new CartDto(
            items,
            cart.TotalPrice(),
            walletBalance,
            cartPrice.TotalAfterCoupon,
            cartPrice.GiftCardCoverage,
            cartPrice.WalletCharge,
            cartPrice.GiftCardWaste);
    }

    private async Task<Payment.Domain.Coupons.Coupon?> GetCouponAsync(string? code)
    {
        if (string.IsNullOrWhiteSpace(code)) return null;

        return await _couponRepository.GetByCodeAsync(code)
            ?? throw new NotFoundException("Coupon does not exist.");
    }

    private async Task<Payment.Domain.GiftCards.GiftCard?> GetGiftCardAsync(Guid? id)
    {
        if (id is null) return null;

        return await _giftCardRepository.GetByIdAsync(id.Value)
            ?? throw new NotFoundException("Gift card does not exist.");
    }
}
