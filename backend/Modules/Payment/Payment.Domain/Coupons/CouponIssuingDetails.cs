namespace Payment.Domain.Coupons;

public sealed record CouponIssuingDetails(
    Guid OwnerId,
    Guid? ProductId,
    int DiscountPercentage,
    DateTime ExpiresAt,
    DateTime CreatedAt);
