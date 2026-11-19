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

    [Fact]
    public void Product_put_in_offer_again_takes_the_new_name_and_price_and_is_in_offer()
    {
        var product = new Product(Guid.NewGuid(), "Šetnja tvrđavom", WellKnownUsers.Explorer, 100);
        product.WithdrawFromOffer();

        product.PutInOfferAgain("Šetnja Petrovaradinom", 250);

        product.Name.Should().Be("Šetnja Petrovaradinom");
        product.Price.Should().Be(250);
        product.IsInOffer.Should().BeTrue();
    }

    [Fact]
    public void Product_cannot_be_put_in_offer_again_with_a_price_less_than_zero()
    {
        var product = new Product(Guid.NewGuid(), "Šetnja tvrđavom", WellKnownUsers.Explorer, 100);

        var putting = () => product.PutInOfferAgain("Šetnja tvrđavom", -1);

        putting.Should().Throw<DomainException>();
    }
}
