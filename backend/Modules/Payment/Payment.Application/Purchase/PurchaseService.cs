using Payment.Application.Carts;
using Payment.Application.Products;
using Payment.Application.PurchaseTokens;
using Payment.Application.Wallets;
using Payment.Domain.Carts;
using Payment.Domain.Wallets;
using Shared.Domain.Exceptions;

namespace Payment.Application.Purchase;

public sealed class PurchaseService
{
    private readonly ICartRepository _cartRepository;
    private readonly IWalletRepository _walletRepository;
    private readonly IProductRepository _productRepository;
    private readonly IPurchaseTokenRepository _purchaseTokenRepository;
    private readonly CartCheckout _cartCheckout;
    private readonly IUnitOfWork _unitOfWork;

    public PurchaseService(
        ICartRepository cartRepository,
        IWalletRepository walletRepository,
        IProductRepository productRepository,
        IPurchaseTokenRepository purchaseTokenRepository,
        CartCheckout cartCheckout,
        IUnitOfWork unitOfWork)
    {
        _cartRepository = cartRepository;
        _walletRepository = walletRepository;
        _productRepository = productRepository;
        _purchaseTokenRepository = purchaseTokenRepository;
        _cartCheckout = cartCheckout;
        _unitOfWork = unitOfWork;
    }

    public async Task PurchaseCartAsync(Guid userId)
    {
        var cart = await _cartRepository.GetByUserIdAsync(userId);
        if (cart is null)
        {
            throw new NotFoundException($"User {userId} has no cart.");
        }

        var wallet = await _walletRepository.GetByUserIdAsync(userId);
        if (wallet is null)
        {
            wallet = new Wallet(userId);
            _walletRepository.Add(wallet);
        }

        var products = await _productRepository.GetByEntityIdsAsync(cart.Items.Select(item => item.EntityId).ToList());

        var tokens = _cartCheckout.Purchase(cart, wallet, products, DateTime.UtcNow);
        foreach (var token in tokens)
        {
            _purchaseTokenRepository.Add(token);
        }

        await _unitOfWork.SaveChangesAsync();
    }
}
