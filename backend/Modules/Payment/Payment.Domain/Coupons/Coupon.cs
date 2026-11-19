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
            throw new DomainException("A coupon discount must be between 1 and 100 percent.");
        }

        if (validUntil <= createdAt)
        {
            throw new DomainException("A coupon validity date must be in the future.");
        }

        OwnerId = ownerId;
        ProductId = productId;
        DiscountPercentage = discountPercentage;
        ValidUntil = validUntil;
        Code = code;
    }
}
