using Payment.Domain.Carts;
using Payment.Domain.Wallets;
using Shared.Tests;

namespace Payment.Tests.Integration.Seeds;

internal static class WalletSeed
{
    public static readonly Guid IdentityDemoExplorer = Guid.Parse("20000000-0000-0000-0000-000000000001");
    public const string IdentityDemoExplorerEmail = "explorer1@explorer.dev";
    public static readonly Guid IdentityDemoSecondExplorer = Guid.Parse("20000000-0000-0000-0000-000000000002");
    public const string IdentityDemoSecondExplorerEmail = "explorer2@explorer.dev";
    public const string IdentityDemoExplorerWithoutWalletEmail = "explorer3@explorer.dev";
    public static readonly Guid ReturningBuyer = Guid.Parse("40000000-0000-0000-0000-000000000001");
    public static readonly Guid UnderfundedBuyer = Guid.Parse("40000000-0000-0000-0000-000000000002");
    public static readonly Guid WalletlessBuyer = Guid.Parse("40000000-0000-0000-0000-000000000003");
    public static readonly DateTime DemoExplorersPurchaseTime = DateTime.UtcNow.Date.AddDays(-1);
    public static readonly DateTime ReturningBuyersPurchaseTime = DateTime.UtcNow.Date.AddDays(-2);

    public static readonly Wallet ExplorersNewWallet = new(WellKnownUsers.Explorer);
    public static readonly Wallet DemoExplorersFundedWallet;
    public static readonly Wallet DemoSecondExplorersWallet = new(IdentityDemoSecondExplorer);
    public static readonly Transaction ExpiredDeposit;
    public static readonly Transaction CancelledDeposit;
    public static readonly Transaction RecentDeposit;
    public static readonly Wallet ReturningBuyersWalletWithPayments;
    public static readonly Wallet UnderfundedBuyersWallet;

    static WalletSeed()
    {
        DemoExplorersFundedWallet = new(IdentityDemoExplorer);
        DemoExplorersFundedWallet.Deposit(WellKnownUsers.Administrator, 500, DateTime.UtcNow.AddHours(-30));
        ExpiredDeposit = DemoExplorersFundedWallet.Transactions[0];
        DemoExplorersFundedWallet.Deposit(WellKnownUsers.Administrator, 200, DateTime.UtcNow.AddHours(-3));
        CancelledDeposit = DemoExplorersFundedWallet.Transactions[1];
        DemoExplorersFundedWallet.CancelTransaction(CancelledDeposit.Id, "Pogrešno unet iznos.", DateTime.UtcNow.AddHours(-2));
        DemoExplorersFundedWallet.Deposit(WellKnownUsers.Administrator, 100, DateTime.UtcNow.AddHours(-1));
        RecentDeposit = DemoExplorersFundedWallet.Transactions[2];
        DemoExplorersFundedWallet.Pay(
            [new CartItem(ProductSeed.PaidTourInOffer.EntityId, ProductSeed.PaidTourInOffer.Name, ProductSeed.PaidTourInOffer.Price)],
            new Dictionary<Guid, Guid> { [ProductSeed.PaidTourInOffer.EntityId] = ProductSeed.PaidTourInOffer.OwnerId },
            DemoExplorersPurchaseTime);

        ReturningBuyersWalletWithPayments = new(ReturningBuyer);
        ReturningBuyersWalletWithPayments.Deposit(WellKnownUsers.Administrator, 500, DateTime.UtcNow.AddDays(-3));
        ReturningBuyersWalletWithPayments.Pay(
            [
                new CartItem(ProductSeed.WithdrawnTour.EntityId, ProductSeed.WithdrawnTour.Name, ProductSeed.WithdrawnTour.Price),
                new CartItem(ProductSeed.FreeTourInOffer.EntityId, ProductSeed.FreeTourInOffer.Name, ProductSeed.FreeTourInOffer.Price)
            ],
            new Dictionary<Guid, Guid>
            {
                [ProductSeed.WithdrawnTour.EntityId] = ProductSeed.WithdrawnTour.OwnerId,
                [ProductSeed.FreeTourInOffer.EntityId] = ProductSeed.FreeTourInOffer.OwnerId
            },
            ReturningBuyersPurchaseTime);

        UnderfundedBuyersWallet = new(UnderfundedBuyer);
        UnderfundedBuyersWallet.Deposit(WellKnownUsers.Administrator, 100, DateTime.UtcNow.AddHours(-5));
    }

    public static Wallet[] All =>
        [ExplorersNewWallet, DemoExplorersFundedWallet, DemoSecondExplorersWallet, ReturningBuyersWalletWithPayments, UnderfundedBuyersWallet];
}
