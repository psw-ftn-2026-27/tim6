using System.Net;
using FluentAssertions;
using Payment.Infrastructure.Persistence;
using Payment.Tests.Integration.Seeds;
using Shared.Tests;
using Xunit;

namespace Payment.Tests.Integration.MyWallet;

public class MyWalletCommandTests : BaseIntegrationTest
{
    public MyWalletCommandTests(PaymentApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Explorer_opening_their_wallet_for_the_first_time_gets_a_wallet_of_their_own()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");
        using var arrangeContext = Factory.CreateContext<PaymentDbContext>();
        var walletCountBefore = arrangeContext.Wallets.Count();

        var response = await client.PostAsync("/api/payment/wallets/mine", null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<PaymentDbContext>();
        assertContext.Wallets.Count().Should().Be(walletCountBefore + 1);
        assertContext.Wallets.Should().ContainSingle(wallet => wallet.UserId == WellKnownUsers.SecondExplorer);
    }

    [Fact]
    public async Task Explorer_opening_their_wallet_again_does_not_get_a_second_wallet()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        using var arrangeContext = Factory.CreateContext<PaymentDbContext>();
        var walletCountBefore = arrangeContext.Wallets.Count();

        var response = await client.PostAsync("/api/payment/wallets/mine", null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<PaymentDbContext>();
        assertContext.Wallets.Count().Should().Be(walletCountBefore);
        var stored = assertContext.Wallets.Single(wallet => wallet.UserId == WellKnownUsers.Explorer);
        stored.Id.Should().Be(WalletSeed.ExplorersNewWallet.Id);
    }

    [Fact]
    public async Task Anonymous_user_cannot_open_a_wallet()
    {
        var client = Factory.CreateClient();

        var response = await client.PostAsync("/api/payment/wallets/mine", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_open_a_wallet()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.PostAsync("/api/payment/wallets/mine", null);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
