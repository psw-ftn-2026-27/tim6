namespace Payment.Application.Cart;

public sealed record CartItemDto(
    Guid EntityId,
    string Name,
    int Price,
    bool IsAvailable,
    int CouponDiscount,
    int PriceAfterCoupon);
