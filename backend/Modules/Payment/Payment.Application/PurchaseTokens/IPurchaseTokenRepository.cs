using Payment.Domain.PurchaseTokens;

namespace Payment.Application.PurchaseTokens;

public interface IPurchaseTokenRepository
{
    Task<bool> ExistsAsync(Guid userId, Guid entityId);
    void Add(PurchaseToken token);
}
