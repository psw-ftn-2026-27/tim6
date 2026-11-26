using Payment.Domain.Products;
using Shared.Domain;
using Shared.Domain.Exceptions;

namespace Payment.Domain.Coupons;

public sealed class Coupon : AggregateRoot
{
    public Guid OwnerId { get; private init; }
    public Guid? ProductId { get; private init; }
    public int DiscountPercentage { get; private set; }
    public DateTime ExpiresAt { get; private init; }
    public DateTime CreatedAt { get; private set; }
    public string Code { get; private init; } = string.Empty;

    private readonly List<Guid> _redeemedByUserIds = [];
    public IReadOnlyList<Guid> RedeemedByUserIds => _redeemedByUserIds.AsReadOnly();

    private Coupon() { }

    public Coupon(
        Guid ownerId,
        Guid? productId,
        int discountPercentage,
        DateTime expiresAt,
        DateTime createdAt) : base(Guid.NewGuid())
    {
        if (discountPercentage is <= 0 or > 100)
        {
            throw new DomainException("Enter a discount between 1% and 100%.");
        }

        if (expiresAt <= createdAt)
        {
            throw new DomainException("Choose a coupon expiry date in the future.");
        }

        OwnerId = ownerId;
        ProductId = productId;
        DiscountPercentage = discountPercentage;
        ExpiresAt = expiresAt;
        CreatedAt = createdAt;
        Code = CreateUniqueCode();
    }

    private static string CreateUniqueCode() => Guid.NewGuid().ToString("N")[..8].ToUpperInvariant();

    public void Redeem(Guid userId, DateTime redeemedAt)
    {
        EnsureCanBeRedeemed(userId, redeemedAt);
        _redeemedByUserIds.Add(userId);
    }

    public bool AppliesTo(Product product)
    {
        if (ProductId is not null)
            return product.Id == ProductId;

        return product.OwnerId == OwnerId;
    }

    public void EnsureCanBeRedeemed(Guid userId, DateTime redeemedAt)
    {
        if (redeemedAt > ExpiresAt)
        {
            throw new DomainException("The coupon has expired.");
        }

        if (_redeemedByUserIds.Contains(userId))
        {
            throw new DomainException("You have already used this coupon.");
        }
    }
}
