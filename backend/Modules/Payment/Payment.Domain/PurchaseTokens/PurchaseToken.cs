using Shared.Domain;

namespace Payment.Domain.PurchaseTokens;

public sealed class PurchaseToken : AggregateRoot
{
    public Guid UserId { get; private set; }
    public Guid EntityId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public DateTime PurchasedAt { get; private set; }

    private PurchaseToken() { }

    public PurchaseToken(Guid userId, Guid entityId, string name, DateTime purchasedAt) : base(Guid.NewGuid())
    {
        UserId = userId;
        EntityId = entityId;
        Name = name;
        PurchasedAt = purchasedAt;
    }
}
