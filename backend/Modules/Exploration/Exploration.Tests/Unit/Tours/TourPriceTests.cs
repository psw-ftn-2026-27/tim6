using Exploration.Domain.Tours;
using FluentAssertions;
using Shared.Domain.Exceptions;
using Xunit;

namespace Exploration.Tests.Unit.Tours;

public class TourPriceTests
{
    [Theory]
    [InlineData(20.5)]
    [InlineData(0.5)]
    [InlineData(0)]
    [InlineData(-5)]
    public void Paid_price_requires_a_whole_amount_of_at_least_one(double amount)
    {
        var creation = () => new TourPrice(false, (decimal)amount);

        creation.Should().Throw<DomainException>();
    }

    [Fact]
    public void Free_price_cannot_have_an_amount()
    {
        var creation = () => new TourPrice(true, 20);

        creation.Should().Throw<DomainException>();
    }

    [Fact]
    public void Free_price_is_offered_for_zero()
    {
        var price = new TourPrice(true, null);

        var offerPrice = price.GetOfferPrice();

        offerPrice.Should().Be(0);
    }
}
