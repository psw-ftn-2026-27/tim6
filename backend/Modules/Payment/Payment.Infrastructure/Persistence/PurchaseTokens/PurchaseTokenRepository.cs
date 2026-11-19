using Microsoft.EntityFrameworkCore;
using Payment.Application.PurchaseTokens;
using Payment.Domain.PurchaseTokens;

namespace Payment.Infrastructure.Persistence.PurchaseTokens;

internal sealed class PurchaseTokenRepository : IPurchaseTokenRepository
{
    private readonly PaymentDbContext _dbContext;

    public PurchaseTokenRepository(PaymentDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<bool> ExistsAsync(Guid userId, Guid entityId)
    {
        return _dbContext.PurchaseTokens.AnyAsync(token => token.UserId == userId && token.EntityId == entityId);
    }

    public void Add(PurchaseToken token)
    {
        _dbContext.PurchaseTokens.Add(token);
    }
}
