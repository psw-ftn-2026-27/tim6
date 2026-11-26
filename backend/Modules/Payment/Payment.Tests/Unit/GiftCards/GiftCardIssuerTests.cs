using FluentAssertions;
using Payment.Domain.GiftCards;
using Payment.Domain.Wallets;
using Shared.Tests;
using Xunit;

namespace Payment.Tests.Unit.GiftCards;

public class GiftCardIssuerTests
{
    [Fact]
    public void Issuing_a_gift_card_spends_the_amount_from_the_sender_wallet()
    {
        var createdAt = DateTime.UtcNow;
        var senderWallet = new Wallet(WellKnownUsers.Explorer);
        senderWallet.Deposit(WellKnownUsers.Administrator, 100, createdAt.AddHours(-1));
        var details = new GiftCardSendingDetails(WellKnownUsers.SecondExplorer, 50, "Srećan put!", createdAt);
        var issuer = new GiftCardIssuer();

        var giftCard = issuer.Issue(senderWallet, details);

        senderWallet.Balance().Should().Be(50);
        giftCard.SenderId.Should().Be(senderWallet.UserId);
        giftCard.RecipientId.Should().Be(details.RecipientId);
        giftCard.Amount.Should().Be(details.Amount);
    }
}
