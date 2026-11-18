using Shared.Tests;

namespace Payment.Tests.Integration.Seeds;

internal static class CartSeed
{
    public static readonly Payment.Domain.Carts.Cart ExplorersCartWithPaidAndWithdrawnTour;
    public static readonly Payment.Domain.Carts.Cart DemoExplorersCartWithFreeTour;

    static CartSeed()
    {
        ExplorersCartWithPaidAndWithdrawnTour = new(WellKnownUsers.Explorer);
        ExplorersCartWithPaidAndWithdrawnTour.AddItem(ProductSeed.PaidTourInOffer.EntityId, ProductSeed.PaidTourInOffer.Name, ProductSeed.PaidTourInOffer.Price);
        ExplorersCartWithPaidAndWithdrawnTour.AddItem(ProductSeed.WithdrawnTour.EntityId, ProductSeed.WithdrawnTour.Name, ProductSeed.WithdrawnTour.Price);

        DemoExplorersCartWithFreeTour = new(WalletSeed.IdentityDemoExplorer);
        DemoExplorersCartWithFreeTour.AddItem(ProductSeed.FreeTourInOffer.EntityId, ProductSeed.FreeTourInOffer.Name, ProductSeed.FreeTourInOffer.Price);
    }

    public static Payment.Domain.Carts.Cart[] All => [ExplorersCartWithPaidAndWithdrawnTour, DemoExplorersCartWithFreeTour];
}
