using Shared.Domain;

namespace Payment.Domain.PurchaseTokens;

public sealed class PurchaseToken : AggregateRoot
{
    public Guid UserId { get; private set; }
    public Guid EntityId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public DateTime PurchasedAt { get; private set; }
    public Guid? CouponId { get; private set; }
    public int CouponDiscount { get; private set; }

    private PurchaseToken() { }

    public PurchaseToken(
        Guid userId,
        Guid entityId,
        string name,
        DateTime purchasedAt,
        Guid? couponId = null,
        int couponDiscount = 0) : base(Guid.NewGuid())
    {
        UserId = userId;
        EntityId = entityId;
        Name = name;
        PurchasedAt = purchasedAt;
        CouponId = couponId;
        CouponDiscount = couponDiscount;
    }
}
