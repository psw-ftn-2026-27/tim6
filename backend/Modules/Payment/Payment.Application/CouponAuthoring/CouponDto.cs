namespace Payment.Application.CouponAuthoring;

public sealed record CouponDto(
    Guid Id,
    Guid OwnerId,
    Guid? ProductId,
    string? ProductName,
    int DiscountPercentage,
    DateTime ValidUntil,
    string Code);
