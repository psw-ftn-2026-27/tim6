using Payment.Application.Purchase;

namespace Payment.Application.PurchaseTokens;

public interface IPurchaseTokenReadRepository
{
    Task<List<PurchaseDto>> GetByUserIdAsync(Guid userId);
    Task<bool> ExistsAsync(Guid userId, Guid entityId);
}
