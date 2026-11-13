using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Payment.Application.WalletManagement;
using Payment.Infrastructure.Persistence;
using Payment.Tests.Integration.Seeds;
using Shared.Tests;
using Xunit;

namespace Payment.Tests.Integration.WalletManagement;

public class WalletManagementCommandTests : BaseIntegrationTest
{
    public WalletManagementCommandTests(PaymentApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Administrator_deposits_an_amount_and_the_deposit_is_stored_with_the_administrator_amount_and_time()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        using var arrangeContext = Factory.CreateContext<PaymentDbContext>();
        var transactionCountBefore = arrangeContext.Wallets
            .Single(wallet => wallet.Id == WalletSeed.ExplorersNewWallet.Id).Transactions.Count;

        var response = await client.PostAsJsonAsync(
            $"/api/payment/wallets/{WalletSeed.ExplorersNewWallet.Id}/deposits", new DepositDto(250));

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<PaymentDbContext>();
        var stored = assertContext.Wallets.Single(wallet => wallet.Id == WalletSeed.ExplorersNewWallet.Id);
        stored.Transactions.Should().HaveCount(transactionCountBefore + 1);
        var deposit = stored.Transactions.Single();
        deposit.AdministratorId.Should().Be(WellKnownUsers.Administrator);
        deposit.Amount.Should().Be(250);
        deposit.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
    }

    [Fact]
    public async Task Deposit_of_zero_is_rejected_and_nothing_is_stored()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        using var arrangeContext = Factory.CreateContext<PaymentDbContext>();
        var transactionCountBefore = arrangeContext.Wallets.SelectMany(wallet => wallet.Transactions).Count();

        var response = await client.PostAsJsonAsync(
            $"/api/payment/wallets/{WalletSeed.ExplorersNewWallet.Id}/deposits", new DepositDto(0));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<PaymentDbContext>();
        assertContext.Wallets.SelectMany(wallet => wallet.Transactions).Count().Should().Be(transactionCountBefore);
    }

    [Fact]
    public async Task Deposit_into_a_wallet_that_does_not_exist_is_rejected()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        using var arrangeContext = Factory.CreateContext<PaymentDbContext>();
        var transactionCountBefore = arrangeContext.Wallets.SelectMany(wallet => wallet.Transactions).Count();

        var response = await client.PostAsJsonAsync($"/api/payment/wallets/{Guid.NewGuid()}/deposits", new DepositDto(250));

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<PaymentDbContext>();
        assertContext.Wallets.SelectMany(wallet => wallet.Transactions).Count().Should().Be(transactionCountBefore);
    }

    [Fact]
    public async Task Anonymous_user_cannot_deposit()
    {
        var client = Factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            $"/api/payment/wallets/{WalletSeed.ExplorersNewWallet.Id}/deposits", new DepositDto(250));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Explorer_cannot_deposit()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.PostAsJsonAsync(
            $"/api/payment/wallets/{WalletSeed.ExplorersNewWallet.Id}/deposits", new DepositDto(250));

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Administrator_cancels_a_deposit_and_it_stays_stored_with_the_mark_and_reason()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.PostAsJsonAsync(
            $"/api/payment/wallets/{WalletSeed.DemoExplorersFundedWallet.Id}/transactions/{WalletSeed.RecentDeposit.Id}/cancel",
            new CancellationDto("Korisnik je uplatio dvaput."));

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<PaymentDbContext>();
        var deposit = assertContext.Wallets
            .Single(wallet => wallet.Id == WalletSeed.DemoExplorersFundedWallet.Id)
            .Transactions.Single(transaction => transaction.Id == WalletSeed.RecentDeposit.Id);
        deposit.IsCancelled.Should().BeTrue();
        deposit.CancellationReason.Should().Be("Korisnik je uplatio dvaput.");
        deposit.CancelledAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
    }

    [Fact]
    public async Task Cancelling_a_deposit_older_than_24_hours_is_rejected_and_the_deposit_stays_active()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.PostAsJsonAsync(
            $"/api/payment/wallets/{WalletSeed.DemoExplorersFundedWallet.Id}/transactions/{WalletSeed.ExpiredDeposit.Id}/cancel",
            new CancellationDto("Uplata greškom."));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<PaymentDbContext>();
        var deposit = assertContext.Wallets
            .Single(wallet => wallet.Id == WalletSeed.DemoExplorersFundedWallet.Id)
            .Transactions.Single(transaction => transaction.Id == WalletSeed.ExpiredDeposit.Id);
        deposit.IsCancelled.Should().BeFalse();
    }

    [Fact]
    public async Task Cancelling_a_deposit_that_does_not_exist_is_rejected()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        using var arrangeContext = Factory.CreateContext<PaymentDbContext>();
        var cancelledCountBefore = arrangeContext.Wallets
            .SelectMany(wallet => wallet.Transactions).Count(transaction => transaction.IsCancelled);

        var response = await client.PostAsJsonAsync(
            $"/api/payment/wallets/{WalletSeed.DemoExplorersFundedWallet.Id}/transactions/{Guid.NewGuid()}/cancel",
            new CancellationDto("Uplata greškom."));

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<PaymentDbContext>();
        assertContext.Wallets
            .SelectMany(wallet => wallet.Transactions).Count(transaction => transaction.IsCancelled)
            .Should().Be(cancelledCountBefore);
    }

    [Fact]
    public async Task Anonymous_user_cannot_cancel_a_deposit()
    {
        var client = Factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            $"/api/payment/wallets/{WalletSeed.DemoExplorersFundedWallet.Id}/transactions/{WalletSeed.RecentDeposit.Id}/cancel",
            new CancellationDto("Uplata greškom."));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Explorer_cannot_cancel_a_deposit()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.PostAsJsonAsync(
            $"/api/payment/wallets/{WalletSeed.DemoExplorersFundedWallet.Id}/transactions/{WalletSeed.RecentDeposit.Id}/cancel",
            new CancellationDto("Uplata greškom."));

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
