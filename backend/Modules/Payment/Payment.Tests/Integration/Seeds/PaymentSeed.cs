namespace Payment.Tests.Integration.Seeds;

internal static class PaymentSeed
{
    public static object[] All => [.. WalletSeed.All, .. ProductSeed.All, .. CartSeed.All, .. PurchaseTokenSeed.All];
}
