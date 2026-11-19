using Payment.Application.Carts;
using Payment.Application.Products;
using Payment.Application.PurchaseTokens;
using Payment.Domain.Carts;
using Payment.Domain.Products;
using Shared.Domain.Exceptions;

namespace Payment.Application.Cart;

public sealed class CartService
{
    private readonly ICartRepository _cartRepository;
    private readonly IProductRepository _productRepository;
    private readonly IPurchaseTokenRepository _purchaseTokenRepository;
    private readonly CartFiller _cartFiller;
    private readonly IUnitOfWork _unitOfWork;

    public CartService(
        ICartRepository cartRepository,
        IProductRepository productRepository,
        IPurchaseTokenRepository purchaseTokenRepository,
        CartFiller cartFiller,
        IUnitOfWork unitOfWork)
    {
        _cartRepository = cartRepository;
        _productRepository = productRepository;
        _purchaseTokenRepository = purchaseTokenRepository;
        _cartFiller = cartFiller;
        _unitOfWork = unitOfWork;
    }

    public async Task AddProductAsync(Guid userId, Guid entityId)
    {
        var cart = await _cartRepository.GetByUserIdAsync(userId);
        if (cart is null)
        {
            cart = new Payment.Domain.Carts.Cart(userId);
            _cartRepository.Add(cart);
        }

        var product = await GetProductAsync(entityId);
        var isAlreadyPurchased = await _purchaseTokenRepository.ExistsAsync(userId, entityId);

        _cartFiller.AddProduct(cart, product, isAlreadyPurchased);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task RemoveItemAsync(Guid userId, Guid entityId)
    {
        var cart = await _cartRepository.GetByUserIdAsync(userId);
        if (cart is null)
        {
            throw new NotFoundException($"User {userId} has no cart.");
        }

        cart.RemoveItem(entityId);
        await _unitOfWork.SaveChangesAsync();
    }

    private async Task<Product> GetProductAsync(Guid entityId)
    {
        var product = await _productRepository.GetByEntityIdAsync(entityId);
        if (product is null)
        {
            throw new NotFoundException($"Product for entity {entityId} does not exist.");
        }

        return product;
    }
}
