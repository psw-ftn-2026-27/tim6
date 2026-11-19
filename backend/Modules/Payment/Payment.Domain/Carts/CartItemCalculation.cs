namespace Payment.Domain.Carts;

public sealed record CartItemCalculation(
    Guid EntityId,
    string Name,
    int OriginalPrice,
    int CouponDiscount,
    int PriceAfterCoupon,
    int WalletCharge,
    Guid? CouponId);
