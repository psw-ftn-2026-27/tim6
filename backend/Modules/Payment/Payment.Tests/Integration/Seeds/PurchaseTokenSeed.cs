using Payment.Domain.PurchaseTokens;

namespace Payment.Tests.Integration.Seeds;

internal static class PurchaseTokenSeed
{
    public static readonly PurchaseToken ReturningBuyersWithdrawnTour = new(WalletSeed.ReturningBuyer, ProductSeed.WithdrawnTour.EntityId, ProductSeed.WithdrawnTour.Name, WalletSeed.ReturningBuyersPurchaseTime);
    public static readonly PurchaseToken ReturningBuyersFreeTour = new(WalletSeed.ReturningBuyer, ProductSeed.FreeTourInOffer.EntityId, ProductSeed.FreeTourInOffer.Name, WalletSeed.ReturningBuyersPurchaseTime);
    public static readonly PurchaseToken DemoExplorersPaidTour = new(WalletSeed.IdentityDemoExplorer, ProductSeed.PaidTourInOffer.EntityId, ProductSeed.PaidTourInOffer.Name, WalletSeed.DemoExplorersPurchaseTime);

    public static PurchaseToken[] All => [ReturningBuyersWithdrawnTour, ReturningBuyersFreeTour, DemoExplorersPaidTour];
}
