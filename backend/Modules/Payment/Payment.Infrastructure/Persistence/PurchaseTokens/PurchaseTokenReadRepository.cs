using Microsoft.EntityFrameworkCore;
using Payment.Application.Purchase;
using Payment.Application.PurchaseTokens;

namespace Payment.Infrastructure.Persistence.PurchaseTokens;

internal sealed class PurchaseTokenReadRepository : IPurchaseTokenReadRepository
{
    private readonly PaymentDbContext _dbContext;

    public PurchaseTokenReadRepository(PaymentDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<List<PurchaseDto>> GetByUserIdAsync(Guid userId)
    {
        return _dbContext.PurchaseTokens
            .Where(token => token.UserId == userId)
            .OrderByDescending(token => token.PurchasedAt)
            .Select(token => new PurchaseDto(token.Id, token.EntityId, token.Name, token.PurchasedAt))
            .ToListAsync();
    }

    public Task<bool> ExistsAsync(Guid userId, Guid entityId)
    {
        return _dbContext.PurchaseTokens.AnyAsync(token => token.UserId == userId && token.EntityId == entityId);
    }
}
