using Microsoft.EntityFrameworkCore;
using Payment.Application.Cart;
using Payment.Application.Carts;

namespace Payment.Infrastructure.Persistence.Carts;

internal sealed class CartReadRepository : ICartReadRepository
{
    private readonly PaymentDbContext _dbContext;

    public CartReadRepository(PaymentDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<List<CartItemDto>> GetItemsAsync(Guid cartId)
    {
        return _dbContext.Carts
            .Where(cart => cart.Id == cartId)
            .SelectMany(cart => cart.Items)
            .Select(item => new CartItemDto(
                item.EntityId,
                item.Name,
                item.Price,
                _dbContext.Products.Any(product => product.EntityId == item.EntityId && product.IsInOffer)))
            .ToListAsync();
    }
}
