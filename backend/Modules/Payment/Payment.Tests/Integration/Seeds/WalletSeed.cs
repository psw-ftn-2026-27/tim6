using Payment.Domain.Wallets;
using Shared.Tests;

namespace Payment.Tests.Integration.Seeds;

internal static class WalletSeed
{
    public static readonly Guid IdentityDemoExplorer = Guid.Parse("20000000-0000-0000-0000-000000000001");
    public const string IdentityDemoExplorerEmail = "explorer1@explorer.dev";

    public static readonly Wallet ExplorersNewWallet = new(WellKnownUsers.Explorer);
    public static readonly Wallet DemoExplorersFundedWallet;
    public static readonly Transaction ExpiredDeposit;
    public static readonly Transaction CancelledDeposit;
    public static readonly Transaction RecentDeposit;

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
    }

    public static Wallet[] All => [ExplorersNewWallet, DemoExplorersFundedWallet];
}
