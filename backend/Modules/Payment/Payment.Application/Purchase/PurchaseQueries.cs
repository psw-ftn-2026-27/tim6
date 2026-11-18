using Payment.Application.PurchaseTokens;

namespace Payment.Application.Purchase;

public sealed class PurchaseQueries
{
    private readonly IPurchaseTokenReadRepository _purchaseTokenReadRepository;

    public PurchaseQueries(IPurchaseTokenReadRepository purchaseTokenReadRepository)
    {
        _purchaseTokenReadRepository = purchaseTokenReadRepository;
    }

    public Task<List<PurchaseDto>> GetMineAsync(Guid userId)
    {
        return _purchaseTokenReadRepository.GetByUserIdAsync(userId);
    }

    public Task<bool> HasPurchasedAsync(Guid userId, Guid entityId)
    {
        return _purchaseTokenReadRepository.ExistsAsync(userId, entityId);
    }
}
