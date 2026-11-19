using Shared.Tests;

namespace Payment.Tests.Integration.Seeds;

internal static class CartSeed
{
    public static readonly Payment.Domain.Carts.Cart ExplorersCartWithPaidAndWithdrawnTour;
    public static readonly Payment.Domain.Carts.Cart DemoExplorersCartWithFreeTour;
    public static readonly Payment.Domain.Carts.Cart ReturningBuyersCartWithPaidTour;
    public static readonly Payment.Domain.Carts.Cart UnderfundedBuyersCartWithPaidTour;
    public static readonly Payment.Domain.Carts.Cart WalletlessBuyersCartWithFreeTour;

    static CartSeed()
    {
        ExplorersCartWithPaidAndWithdrawnTour = new(WellKnownUsers.Explorer);
        ExplorersCartWithPaidAndWithdrawnTour.AddItem(ProductSeed.PaidTourInOffer.EntityId, ProductSeed.PaidTourInOffer.Name, ProductSeed.PaidTourInOffer.Price);
        ExplorersCartWithPaidAndWithdrawnTour.AddItem(ProductSeed.WithdrawnTour.EntityId, ProductSeed.WithdrawnTour.Name, ProductSeed.WithdrawnTour.Price);

        DemoExplorersCartWithFreeTour = new(WalletSeed.IdentityDemoExplorer);
        DemoExplorersCartWithFreeTour.AddItem(ProductSeed.FreeTourInOffer.EntityId, ProductSeed.FreeTourInOffer.Name, ProductSeed.FreeTourInOffer.Price);

        ReturningBuyersCartWithPaidTour = new(WalletSeed.ReturningBuyer);
        ReturningBuyersCartWithPaidTour.AddItem(ProductSeed.PaidTourInOffer.EntityId, ProductSeed.PaidTourInOffer.Name, ProductSeed.PaidTourInOffer.Price);

        UnderfundedBuyersCartWithPaidTour = new(WalletSeed.UnderfundedBuyer);
        UnderfundedBuyersCartWithPaidTour.AddItem(ProductSeed.PaidTourInOffer.EntityId, ProductSeed.PaidTourInOffer.Name, ProductSeed.PaidTourInOffer.Price);

        WalletlessBuyersCartWithFreeTour = new(WalletSeed.WalletlessBuyer);
        WalletlessBuyersCartWithFreeTour.AddItem(ProductSeed.FreeTourInOffer.EntityId, ProductSeed.FreeTourInOffer.Name, ProductSeed.FreeTourInOffer.Price);
    }

    public static Payment.Domain.Carts.Cart[] All =>
    [
        ExplorersCartWithPaidAndWithdrawnTour,
        DemoExplorersCartWithFreeTour,
        ReturningBuyersCartWithPaidTour,
        UnderfundedBuyersCartWithPaidTour,
        WalletlessBuyersCartWithFreeTour
    ];
}
