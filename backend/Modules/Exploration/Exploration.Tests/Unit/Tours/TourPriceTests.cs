using Exploration.Domain.Tours;
using FluentAssertions;
using Shared.Domain.Exceptions;
using Xunit;

namespace Exploration.Tests.Unit.Tours;

public class TourPriceTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void Paid_price_requires_an_amount_greater_than_zero(int amount)
    {
        var creation = () => new TourPrice(false, amount);

        creation.Should().Throw<DomainException>();
    }

    [Fact]
    public void Free_price_cannot_have_an_amount()
    {
        var creation = () => new TourPrice(true, 20);

        creation.Should().Throw<DomainException>();
    }
}
