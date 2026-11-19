namespace Payment.Application.CouponAuthoring;

public sealed record CreateCouponDto(Guid? ProductId, int DiscountPercentage, DateTime ValidUntil);
