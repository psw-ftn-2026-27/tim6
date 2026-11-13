using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Payment.Application.MyWallet;
using Shared.Tests;
using Xunit;

namespace Payment.Tests.Integration.MyWallet;

public class MyWalletQueryTests : BaseIntegrationTest
{
    public MyWalletQueryTests(PaymentApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Explorer_with_a_newly_opened_wallet_sees_a_zero_balance_and_no_transactions()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var wallet = await client.GetFromJsonAsync<WalletDto>("/api/payment/wallets/mine", JsonOptions);

        wallet!.Balance.Should().Be(0);
        wallet.Transactions.Should().BeEmpty();
    }

    [Fact]
    public async Task Explorer_who_never_opened_a_wallet_is_told_it_does_not_exist()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");

        var response = await client.GetAsync("/api/payment/wallets/mine");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
