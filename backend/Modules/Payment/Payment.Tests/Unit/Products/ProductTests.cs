using FluentAssertions;
using Payment.Domain.Products;
using Shared.Domain.Exceptions;
using Shared.Tests;
using Xunit;

namespace Payment.Tests.Unit.Products;

public class ProductTests
{
    [Theory]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Product_with_a_price_less_than_zero_cannot_be_created(int price)
    {
        var creation = () => new Product(Guid.NewGuid(), "Šetnja tvrđavom", WellKnownUsers.Explorer, price);

        creation.Should().Throw<DomainException>();
    }

    [Fact]
    public void Product_with_a_price_of_zero_is_created_in_the_offer()
    {
        var product = new Product(Guid.NewGuid(), "Šetnja tvrđavom", WellKnownUsers.Explorer, 0);

        product.IsInOffer.Should().BeTrue();
        product.Price.Should().Be(0);
    }
}
