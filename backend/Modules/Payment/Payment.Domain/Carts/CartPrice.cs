namespace Payment.Domain.Carts;

public sealed record CartPrice(
    IReadOnlyList<CartItemPrice> Items,
    int TotalAfterCoupon,
    int GiftCardCoverage,
    int WalletCharge,
    int GiftCardWaste);
