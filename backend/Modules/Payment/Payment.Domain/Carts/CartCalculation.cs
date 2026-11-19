namespace Payment.Domain.Carts;

public sealed record CartCalculation(
    IReadOnlyList<CartItemCalculation> Items,
    int TotalAfterCoupon,
    int GiftCardCoverage,
    int WalletCharge,
    int GiftCardWaste);
