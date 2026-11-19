using Shared.Domain;

namespace Payment.Domain.Wallets;

public sealed class Payment : Entity
{
    public Guid EntityId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public Guid OwnerId { get; private set; }
    public int Price { get; private set; }
    public DateTime PaidAt { get; private set; }

    private Payment() { }

    internal Payment(Guid entityId, string name, Guid ownerId, int price, DateTime paidAt) : base(Guid.NewGuid())
    {
        EntityId = entityId;
        Name = name;
        OwnerId = ownerId;
        Price = price;
        PaidAt = paidAt;
    }
}
