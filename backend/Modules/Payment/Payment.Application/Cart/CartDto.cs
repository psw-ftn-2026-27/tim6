namespace Payment.Application.Cart;

public sealed record CartDto(
    List<CartItemDto> Items,
    int TotalPrice,
    int WalletBalance,
    int TotalAfterCoupon,
    int GiftCardCoverage,
    int WalletCharge,
    int GiftCardWaste);
