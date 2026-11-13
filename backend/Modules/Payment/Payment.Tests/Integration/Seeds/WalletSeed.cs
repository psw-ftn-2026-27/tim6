using Payment.Domain.Wallets;
using Shared.Tests;

namespace Payment.Tests.Integration.Seeds;

internal static class WalletSeed
{
    public static readonly Wallet ExplorersNewWallet = new(WellKnownUsers.Explorer);

    public static Wallet[] All => [ExplorersNewWallet];
}
