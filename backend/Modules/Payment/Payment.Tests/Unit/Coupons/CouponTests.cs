using FluentAssertions;
using Payment.Domain.Coupons;
using Shared.Domain.Exceptions;
using Shared.Tests;
using Xunit;

namespace Payment.Tests.Unit.Coupons;

public class CouponTests
{
    private static readonly DateTime CreatedAt = new(2026, 11, 18, 12, 0, 0, DateTimeKind.Utc);

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(101)]
    public void Coupon_with_a_discount_outside_the_allowed_range_cannot_be_created(int discountPercentage)
    {
        var creation = () => CreateCoupon(discountPercentage, CreatedAt.AddDays(1));

        creation.Should().Throw<DomainException>();
    }

    [Fact]
    public void Coupon_with_a_discount_of_one_hundred_percent_can_be_created()
    {
        var coupon = CreateCoupon(100, CreatedAt.AddDays(1));

        coupon.DiscountPercentage.Should().Be(100);
    }

    [Fact]
    public void Coupon_generates_an_eight_character_uppercase_hexadecimal_code()
    {
        var coupon = CreateCoupon(20, CreatedAt.AddDays(1));

        coupon.Code.Should().MatchRegex("^[0-9A-F]{8}$");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Coupon_with_a_validity_date_that_is_not_in_the_future_cannot_be_created(int dayOffset)
    {
        var creation = () => CreateCoupon(20, CreatedAt.AddDays(dayOffset));

        creation.Should().Throw<DomainException>();
    }

    [Fact]
    public void Expired_coupon_cannot_be_redeemed()
    {
        var coupon = CreateCoupon(20, CreatedAt.AddDays(1));

        var redemption = () => coupon.Redeem(WellKnownUsers.Explorer, CreatedAt.AddDays(2));

        redemption.Should().Throw<DomainException>();
    }

    [Fact]
    public void Coupon_cannot_be_redeemed_twice_by_the_same_buyer_but_another_buyer_can_redeem_it()
    {
        var coupon = CreateCoupon(20, CreatedAt.AddDays(2));
        coupon.Redeem(WellKnownUsers.Explorer, CreatedAt.AddHours(1));

        var repeatedRedemption = () => coupon.Redeem(WellKnownUsers.Explorer, CreatedAt.AddHours(2));
        var otherBuyerRedemption = () => coupon.Redeem(WellKnownUsers.SecondExplorer, CreatedAt.AddHours(2));

        repeatedRedemption.Should().Throw<DomainException>();
        otherBuyerRedemption.Should().NotThrow();
        coupon.RedeemedByUserIds.Should().BeEquivalentTo(
            new[] { WellKnownUsers.Explorer, WellKnownUsers.SecondExplorer });
    }

    private static Coupon CreateCoupon(int discountPercentage, DateTime expiresAt)
    {
        return new Coupon(
            WellKnownUsers.Explorer,
            Guid.NewGuid(),
            discountPercentage,
            expiresAt,
            CreatedAt);
    }
}
