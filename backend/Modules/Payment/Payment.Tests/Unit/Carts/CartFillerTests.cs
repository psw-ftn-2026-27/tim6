using FluentAssertions;
using Payment.Domain.Carts;
using Payment.Domain.Products;
using Shared.Domain.Exceptions;
using Shared.Tests;
using Xunit;

namespace Payment.Tests.Unit.Carts;

public class CartFillerTests
{
    private readonly CartFiller _cartFiller = new();

    [Fact]
    public void Product_that_is_not_in_the_offer_cannot_be_added_to_the_cart()
    {
        var cart = new Cart(WellKnownUsers.Explorer);
        var product = new Product(Guid.NewGuid(), "Bački salaši", WellKnownUsers.SecondExplorer, 150);
        product.WithdrawFromOffer();

        var addition = () => _cartFiller.AddProduct(cart, product);

        addition.Should().Throw<DomainException>();
    }
}
