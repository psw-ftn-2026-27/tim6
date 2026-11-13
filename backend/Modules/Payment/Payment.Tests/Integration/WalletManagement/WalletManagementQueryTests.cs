using System.Net.Http.Json;
using FluentAssertions;
using Payment.Application.WalletManagement;
using Payment.Tests.Integration.Seeds;
using Shared.Domain;
using Shared.Tests;
using Xunit;

namespace Payment.Tests.Integration.WalletManagement;

public class WalletManagementQueryTests : BaseIntegrationTest
{
    public WalletManagementQueryTests(PaymentApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Wallet_list_shows_the_users_email_from_identity_and_a_balance_without_cancelled_deposits()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var wallets = await client.GetFromJsonAsync<PageResult<WalletSummaryDto>>("/api/payment/wallets", JsonOptions);

        var wallet = wallets!.Items.Single(item => item.Id == WalletSeed.DemoExplorersFundedWallet.Id);
        wallet.Email.Should().Be(WalletSeed.IdentityDemoExplorerEmail);
        wallet.Balance.Should().Be(WalletSeed.DemoExplorersFundedWallet.Balance());
    }

    [Fact]
    public async Task Wallet_of_a_user_unknown_to_identity_is_listed_with_its_balance_and_no_email()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var wallets = await client.GetFromJsonAsync<PageResult<WalletSummaryDto>>("/api/payment/wallets", JsonOptions);

        var wallet = wallets!.Items.Single(item => item.Id == WalletSeed.ExplorersNewWallet.Id);
        wallet.Email.Should().BeNull();
        wallet.Balance.Should().Be(WalletSeed.ExplorersNewWallet.Balance());
    }

    [Fact]
    public async Task Opened_wallet_shows_the_email_balance_and_newest_first_transactions_with_the_cancelled_deposit_marked()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var wallet = await client.GetFromJsonAsync<WalletDetailsDto>(
            $"/api/payment/wallets/{WalletSeed.DemoExplorersFundedWallet.Id}", JsonOptions);

        wallet!.Email.Should().Be(WalletSeed.IdentityDemoExplorerEmail);
        wallet.Balance.Should().Be(WalletSeed.DemoExplorersFundedWallet.Balance());
        wallet.Transactions.Select(transaction => transaction.Id)
            .Should().BeEquivalentTo(WalletSeed.DemoExplorersFundedWallet.Transactions.Select(transaction => transaction.Id));
        wallet.Transactions.Should().BeInDescendingOrder(transaction => transaction.CreatedAt);
        var cancelled = wallet.Transactions.Single(transaction => transaction.Id == WalletSeed.CancelledDeposit.Id);
        cancelled.IsCancelled.Should().BeTrue();
        cancelled.CancelledAt.Should().NotBeNull();
        cancelled.CancellationReason.Should().Be(WalletSeed.CancelledDeposit.CancellationReason);
    }
}
