using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Payment.Application.GiftCardSending;
using Payment.Infrastructure.Persistence;
using Payment.Tests.Integration.Seeds;
using Shared.Tests;
using Xunit;

namespace Payment.Tests.Integration.GiftCardSending;

public class GiftCardSendingCommandTests : BaseIntegrationTest
{
    public GiftCardSendingCommandTests(PaymentApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Explorer_sending_a_gift_card_stores_the_card_and_spends_the_amount_in_one_request()
    {
        const int amount = 100;
        var client = Factory.CreateClientFor(WalletSeed.IdentityDemoExplorer, "explorer");
        var dto = new CreateGiftCardDto(WalletSeed.IdentityDemoSecondExplorerEmail, amount, "Srećan put!");
        await using var arrangeContext = Factory.CreateContext<PaymentDbContext>();
        var giftCardCountBefore = arrangeContext.GiftCards.Count();
        var balanceBefore = arrangeContext.Wallets.Single(
            wallet => wallet.UserId == WalletSeed.IdentityDemoExplorer).Balance();

        var response = await client.PostAsJsonAsync("/api/payment/gift-cards", dto);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var giftCardId = await response.Content.ReadFromJsonAsync<Guid>(JsonOptions);
        await using var assertContext = Factory.CreateContext<PaymentDbContext>();
        assertContext.GiftCards.Count().Should().Be(giftCardCountBefore + 1);
        assertContext.GiftCards.Should().ContainSingle(giftCard =>
            giftCard.Id == giftCardId &&
            giftCard.SenderId == WalletSeed.IdentityDemoExplorer &&
            giftCard.RecipientId == WalletSeed.IdentityDemoSecondExplorer &&
            giftCard.Amount == amount);
        assertContext.Wallets.Single(
            wallet => wallet.UserId == WalletSeed.IdentityDemoExplorer).Balance().Should().Be(balanceBefore - amount);
    }

    [Fact]
    public async Task Missing_recipient_prevents_a_gift_card_and_leaves_payment_data_unchanged()
    {
        var client = Factory.CreateClientFor(WalletSeed.IdentityDemoExplorer, "explorer");
        var dto = new CreateGiftCardDto("missing@explorer.dev", 100, "Srećan put!");
        await using var arrangeContext = Factory.CreateContext<PaymentDbContext>();
        var giftCardCountBefore = arrangeContext.GiftCards.Count();
        var balanceBefore = arrangeContext.Wallets.Single(
            wallet => wallet.UserId == WalletSeed.IdentityDemoExplorer).Balance();

        var response = await client.PostAsJsonAsync("/api/payment/gift-cards", dto);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        await using var assertContext = Factory.CreateContext<PaymentDbContext>();
        assertContext.GiftCards.Count().Should().Be(giftCardCountBefore);
        assertContext.Wallets.Single(
            wallet => wallet.UserId == WalletSeed.IdentityDemoExplorer).Balance().Should().Be(balanceBefore);
    }

    [Fact]
    public async Task Insufficient_balance_prevents_a_gift_card_and_leaves_payment_data_unchanged()
    {
        var client = Factory.CreateClientFor(WalletSeed.IdentityDemoExplorer, "explorer");
        var dto = new CreateGiftCardDto(WalletSeed.IdentityDemoSecondExplorerEmail, 1000, "Srećan put!");
        await using var arrangeContext = Factory.CreateContext<PaymentDbContext>();
        var giftCardCountBefore = arrangeContext.GiftCards.Count();
        var balanceBefore = arrangeContext.Wallets.Single(
            wallet => wallet.UserId == WalletSeed.IdentityDemoExplorer).Balance();

        var response = await client.PostAsJsonAsync("/api/payment/gift-cards", dto);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        await using var assertContext = Factory.CreateContext<PaymentDbContext>();
        assertContext.GiftCards.Count().Should().Be(giftCardCountBefore);
        assertContext.Wallets.Single(
            wallet => wallet.UserId == WalletSeed.IdentityDemoExplorer).Balance().Should().Be(balanceBefore);
    }

    [Fact]
    public async Task Anonymous_user_cannot_send_a_gift_card()
    {
        var client = Factory.CreateClient();
        var dto = new CreateGiftCardDto(WalletSeed.IdentityDemoSecondExplorerEmail, 100, "Srećan put!");

        var response = await client.PostAsJsonAsync("/api/payment/gift-cards", dto);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_send_a_gift_card()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var dto = new CreateGiftCardDto(WalletSeed.IdentityDemoSecondExplorerEmail, 100, "Srećan put!");

        var response = await client.PostAsJsonAsync("/api/payment/gift-cards", dto);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
