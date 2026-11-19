namespace Payment.Domain.Coupons;

public sealed record CouponIssueDetails(
    Guid OwnerId,
    Guid? ProductId,
    int DiscountPercentage,
    DateTime ValidUntil,
    DateTime CreatedAt);
