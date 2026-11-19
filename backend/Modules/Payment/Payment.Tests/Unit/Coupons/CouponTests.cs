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

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Coupon_with_a_validity_date_that_is_not_in_the_future_cannot_be_created(int dayOffset)
    {
        var creation = () => CreateCoupon(20, CreatedAt.AddDays(dayOffset));

        creation.Should().Throw<DomainException>();
    }

    private static Coupon CreateCoupon(int discountPercentage, DateTime validUntil)
    {
        return new Coupon(
            WellKnownUsers.Explorer,
            Guid.NewGuid(),
            discountPercentage,
            validUntil,
            "ABC12345",
            CreatedAt);
    }
}
