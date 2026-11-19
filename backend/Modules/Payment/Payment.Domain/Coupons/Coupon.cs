using Shared.Domain;
using Shared.Domain.Exceptions;

namespace Payment.Domain.Coupons;

public sealed class Coupon : AggregateRoot
{
    public Guid OwnerId { get; private set; }
    public Guid? ProductId { get; private set; }
    public int DiscountPercentage { get; private set; }
    public DateTime ValidUntil { get; private set; }
    public string Code { get; private set; } = string.Empty;
    public List<Guid> RedeemedByUserIds { get; private set; } = [];

    private Coupon() { }

    public Coupon(
        Guid ownerId,
        Guid? productId,
        int discountPercentage,
        DateTime validUntil,
        string code,
        DateTime createdAt) : base(Guid.NewGuid())
    {
        if (discountPercentage is <= 0 or > 100)
        {
            throw new DomainException("Enter a discount between 1% and 100%.");
        }

        if (validUntil <= createdAt)
        {
            throw new DomainException("Choose a coupon expiry date in the future.");
        }

        OwnerId = ownerId;
        ProductId = productId;
        DiscountPercentage = discountPercentage;
        ValidUntil = validUntil;
        Code = code;
    }

    public void Redeem(Guid userId, DateTime redeemedAt)
    {
        EnsureCanBeRedeemed(userId, redeemedAt);
        RedeemedByUserIds.Add(userId);
    }

    public void EnsureCanBeRedeemed(Guid userId, DateTime redeemedAt)
    {
        if (redeemedAt > ValidUntil)
        {
            throw new DomainException("The coupon has expired.");
        }

        if (RedeemedByUserIds.Contains(userId))
        {
            throw new DomainException("You have already used this coupon.");
        }
    }
}
