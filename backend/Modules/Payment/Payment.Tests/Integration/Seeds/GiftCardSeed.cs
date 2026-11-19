using Payment.Domain.GiftCards;

namespace Payment.Tests.Integration.Seeds;

internal static class GiftCardSeed
{
    public static readonly GiftCard ValidCardSentByDemoExplorer;
    public static readonly GiftCard ExpiredCardReceivedByDemoExplorer;
    public static readonly GiftCard RedeemedCardReceivedByDemoExplorer;

    static GiftCardSeed()
    {
        ValidCardSentByDemoExplorer = new GiftCard(
            WalletSeed.IdentityDemoExplorer,
            WalletSeed.IdentityDemoSecondExplorer,
            100,
            "Srećan put!",
            DateTime.UtcNow.AddDays(-1));
        ExpiredCardReceivedByDemoExplorer = new GiftCard(
            WalletSeed.IdentityDemoSecondExplorer,
            WalletSeed.IdentityDemoExplorer,
            50,
            "Pozdrav sa putovanja!",
            DateTime.UtcNow.AddYears(-4));
        RedeemedCardReceivedByDemoExplorer = new GiftCard(
            WalletSeed.IdentityDemoSecondExplorer,
            WalletSeed.IdentityDemoExplorer,
            75,
            "Uživaj!",
            DateTime.UtcNow.AddYears(-4));
        RedeemedCardReceivedByDemoExplorer.Redeem(DateTime.UtcNow.AddYears(-3));
    }

    public static GiftCard[] All =>
    [
        ValidCardSentByDemoExplorer,
        ExpiredCardReceivedByDemoExplorer,
        RedeemedCardReceivedByDemoExplorer
    ];
}
