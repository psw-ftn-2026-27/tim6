using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Payment.Application.MyWallet;
using Payment.Tests.Integration.Seeds;
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

    [Fact]
    public async Task Buyer_sees_their_payments_with_name_price_and_time_beside_their_deposits_and_without_others_payments()
    {
        var client = Factory.CreateClientFor(WalletSeed.ReturningBuyer, "explorer");
        var seeded = WalletSeed.ReturningBuyersWalletWithPayments;
        var expectedPayments = seeded.Payments
            .Select(payment => new PaymentDto(payment.Id, payment.EntityId, payment.Name, payment.Price, payment.PaidAt));

        var wallet = await client.GetFromJsonAsync<WalletDto>("/api/payment/wallets/mine", JsonOptions);

        wallet!.Payments.Should().BeEquivalentTo(expectedPayments);
        wallet.Transactions.Select(transaction => transaction.Id)
            .Should().BeEquivalentTo(seeded.Transactions.Select(transaction => transaction.Id));
    }
}
