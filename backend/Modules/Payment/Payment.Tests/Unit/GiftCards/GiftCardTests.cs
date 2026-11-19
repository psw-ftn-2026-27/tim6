using FluentAssertions;
using Payment.Domain.GiftCards;
using Shared.Domain.Exceptions;
using Shared.Tests;
using Xunit;

namespace Payment.Tests.Unit.GiftCards;

public class GiftCardTests
{
    private static readonly DateTime CreatedAt = new(2026, 11, 1, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Gift_card_cannot_have_the_same_sender_and_recipient()
    {
        var creation = () => new GiftCard(
            WellKnownUsers.Explorer,
            WellKnownUsers.Explorer,
            100,
            "Srećan put!",
            CreatedAt);

        creation.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData(9)]
    [InlineData(1001)]
    public void Gift_card_amount_outside_the_allowed_range_is_rejected(int amount)
    {
        var creation = () => CreateGiftCard(amount, "Srećan put!");

        creation.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData(10)]
    [InlineData(1000)]
    public void Gift_card_amount_accepts_the_allowed_boundaries(int amount)
    {
        var giftCard = CreateGiftCard(amount, "Srećan put!");

        giftCard.Amount.Should().Be(amount);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Gift_card_requires_a_message(string message)
    {
        var creation = () => CreateGiftCard(100, message);

        creation.Should().Throw<DomainException>();
    }

    [Fact]
    public void Gift_card_rejects_a_message_longer_than_200_characters()
    {
        var creation = () => CreateGiftCard(100, new string('a', 201));

        creation.Should().Throw<DomainException>();
    }

    [Fact]
    public void Gift_card_is_valid_until_it_expires_and_expired_afterwards()
    {
        var giftCard = CreateGiftCard(100, "Srećan put!");

        giftCard.Status(giftCard.ExpiresAt.AddTicks(-1)).Should().Be(GiftCardStatus.Valid);
        giftCard.Status(giftCard.ExpiresAt).Should().Be(GiftCardStatus.Expired);
    }

    [Fact]
    public void Redeemed_gift_card_stays_redeemed_after_its_expiration_date()
    {
        var giftCard = CreateGiftCard(100, "Srećan put!");
        giftCard.Redeem(CreatedAt.AddDays(1));

        var status = giftCard.Status(giftCard.ExpiresAt.AddDays(1));

        status.Should().Be(GiftCardStatus.Redeemed);
    }

    [Fact]
    public void Gift_card_can_only_be_redeemed_by_its_recipient_while_valid_and_not_previously_redeemed()
    {
        var giftCard = CreateGiftCard(100, "Srećan put!");

        var foreignRedemption = () => giftCard.Redeem(WellKnownUsers.Explorer, CreatedAt.AddDays(1));
        var validRedemption = () => giftCard.Redeem(WellKnownUsers.SecondExplorer, CreatedAt.AddDays(1));

        foreignRedemption.Should().Throw<DomainException>();
        validRedemption.Should().NotThrow();
        giftCard.IsRedeemed.Should().BeTrue();
        giftCard.RedeemedAt.Should().Be(CreatedAt.AddDays(1));
    }

    [Fact]
    public void Expired_or_previously_redeemed_gift_card_cannot_be_redeemed()
    {
        var expiredGiftCard = CreateGiftCard(100, "Srećan put!");
        var redeemedGiftCard = CreateGiftCard(100, "Uživaj!");
        redeemedGiftCard.Redeem(WellKnownUsers.SecondExplorer, CreatedAt.AddDays(1));

        var expiredRedemption = () => expiredGiftCard.Redeem(WellKnownUsers.SecondExplorer, expiredGiftCard.ExpiresAt);
        var repeatedRedemption = () => redeemedGiftCard.Redeem(WellKnownUsers.SecondExplorer, CreatedAt.AddDays(2));

        expiredRedemption.Should().Throw<DomainException>();
        repeatedRedemption.Should().Throw<DomainException>();
    }

    private static GiftCard CreateGiftCard(int amount, string message)
    {
        return new GiftCard(
            WellKnownUsers.Explorer,
            WellKnownUsers.SecondExplorer,
            amount,
            message,
            CreatedAt);
    }
}
