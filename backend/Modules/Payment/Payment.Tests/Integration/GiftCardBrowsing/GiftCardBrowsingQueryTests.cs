using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Payment.Application.GiftCards;
using Payment.Domain.GiftCards;
using Payment.Tests.Integration.Seeds;
using Shared.Domain;
using Xunit;

namespace Payment.Tests.Integration.GiftCardBrowsing;

public class GiftCardBrowsingQueryTests : BaseIntegrationTest
{
    public GiftCardBrowsingQueryTests(PaymentApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Explorer_sees_their_sent_gift_cards_with_recipient_email_and_status()
    {
        var client = Factory.CreateClientFor(WalletSeed.IdentityDemoExplorer, "explorer");

        var response = await client.GetAsync("/api/payment/gift-cards/sent?page=1&pageSize=20");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var page = await response.Content.ReadFromJsonAsync<PageResult<GiftCardDto>>(JsonOptions);
        page!.Items.Should().ContainSingle(giftCard =>
            giftCard.Id == GiftCardSeed.ValidCardSentByDemoExplorer.Id &&
            giftCard.RecipientEmail == WalletSeed.IdentityDemoSecondExplorerEmail &&
            giftCard.Status == GiftCardStatus.Valid);
    }

    [Fact]
    public async Task Explorer_sees_their_received_gift_cards_with_sender_email_and_status()
    {
        var client = Factory.CreateClientFor(WalletSeed.IdentityDemoExplorer, "explorer");

        var response = await client.GetAsync("/api/payment/gift-cards/received?page=1&pageSize=20");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var page = await response.Content.ReadFromJsonAsync<PageResult<GiftCardDto>>(JsonOptions);
        page!.Items.Should().Contain(giftCard =>
            giftCard.Id == GiftCardSeed.ExpiredCardReceivedByDemoExplorer.Id &&
            giftCard.SenderEmail == WalletSeed.IdentityDemoSecondExplorerEmail &&
            giftCard.Status == GiftCardStatus.Expired);
        page.Items.Should().Contain(giftCard =>
            giftCard.Id == GiftCardSeed.RedeemedCardReceivedByDemoExplorer.Id &&
            giftCard.SenderEmail == WalletSeed.IdentityDemoSecondExplorerEmail &&
            giftCard.Status == GiftCardStatus.Redeemed);
    }
}
