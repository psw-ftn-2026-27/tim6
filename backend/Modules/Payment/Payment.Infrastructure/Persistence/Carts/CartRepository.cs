using Microsoft.EntityFrameworkCore;
using Payment.Application.Carts;
using Payment.Domain.Carts;

namespace Payment.Infrastructure.Persistence.Carts;

internal sealed class CartRepository : ICartRepository
{
    private readonly PaymentDbContext _dbContext;

    public CartRepository(PaymentDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Cart?> GetByUserIdAsync(Guid userId)
    {
        return _dbContext.Carts.FirstOrDefaultAsync(cart => cart.UserId == userId);
    }

    public void Add(Cart cart)
    {
        _dbContext.Carts.Add(cart);
    }
}
