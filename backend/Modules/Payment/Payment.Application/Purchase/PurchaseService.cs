using Payment.Application.Carts;
using Payment.Application.Coupons;
using Payment.Application.GiftCards;
using Payment.Application.Products;
using Payment.Application.PurchaseTokens;
using Payment.Application.Wallets;
using Payment.Domain.Carts;
using Payment.Domain.Coupons;
using Payment.Domain.GiftCards;
using Shared.Domain.Exceptions;

namespace Payment.Application.Purchase;

public sealed class PurchaseService
{
    private readonly ICartRepository _cartRepository;
    private readonly IWalletRepository _walletRepository;
    private readonly IProductRepository _productRepository;
    private readonly IPurchaseTokenRepository _purchaseTokenRepository;
    private readonly ICouponRepository _couponRepository;
    private readonly IGiftCardRepository _giftCardRepository;
    private readonly CartCheckout _cartCheckout;
    private readonly IUnitOfWork _unitOfWork;

    public PurchaseService(
        ICartRepository cartRepository,
        IWalletRepository walletRepository,
        IProductRepository productRepository,
        IPurchaseTokenRepository purchaseTokenRepository,
        ICouponRepository couponRepository,
        IGiftCardRepository giftCardRepository,
        CartCheckout cartCheckout,
        IUnitOfWork unitOfWork)
    {
        _cartRepository = cartRepository;
        _walletRepository = walletRepository;
        _productRepository = productRepository;
        _purchaseTokenRepository = purchaseTokenRepository;
        _couponRepository = couponRepository;
        _giftCardRepository = giftCardRepository;
        _cartCheckout = cartCheckout;
        _unitOfWork = unitOfWork;
    }

    public async Task PurchaseCartAsync(Guid userId, string? couponCode = null, Guid? giftCardId = null)
    {
        var cart = await _cartRepository.GetByUserIdAsync(userId);
        if (cart is null)
        {
            throw new NotFoundException($"User {userId} has no cart.");
        }

        var wallet = await _walletRepository.GetByUserIdAsync(userId)
            ?? throw new NotFoundException("You must open a wallet before continuing.");

        var products = await _productRepository.GetByEntityIdsAsync(cart.Items.Select(item => item.EntityId).ToList());
        var coupon = await GetCouponAsync(couponCode);
        var giftCard = await GetGiftCardAsync(giftCardId);

        var tokens = _cartCheckout.Purchase(cart, wallet, products, coupon, giftCard, DateTime.UtcNow);
        foreach (var token in tokens)
            _purchaseTokenRepository.Add(token);

        await _unitOfWork.SaveChangesAsync();
    }

    private async Task<Coupon?> GetCouponAsync(string? code)
    {
        if (string.IsNullOrWhiteSpace(code)) return null;
        return await _couponRepository.GetByCodeAsync(code)
            ?? throw new NotFoundException("Coupon does not exist.");
    }

    private async Task<GiftCard?> GetGiftCardAsync(Guid? id)
    {
        if (id is null) return null;
        return await _giftCardRepository.GetByIdAsync(id.Value)
            ?? throw new NotFoundException("Gift card does not exist.");
    }
}
