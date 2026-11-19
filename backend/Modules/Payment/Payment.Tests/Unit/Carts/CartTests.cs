using FluentAssertions;
using Payment.Domain.Carts;
using Shared.Domain.Exceptions;
using Shared.Tests;
using Xunit;

namespace Payment.Tests.Unit.Carts;

public class CartTests
{
    [Fact]
    public void Total_price_of_a_cart_is_the_sum_of_its_item_prices()
    {
        var cart = new Cart(WellKnownUsers.Explorer);
        cart.AddItem(Guid.NewGuid(), "Šetnja tvrđavom", 0);
        cart.AddItem(Guid.NewGuid(), "Vinogradi Sremskih Karlovaca", 300);
        cart.AddItem(Guid.NewGuid(), "Bački salaši", 150);

        var totalPrice = cart.TotalPrice();

        totalPrice.Should().Be(450);
    }

    [Fact]
    public void Product_already_in_the_cart_cannot_be_added_again()
    {
        var cart = new Cart(WellKnownUsers.Explorer);
        var entityId = Guid.NewGuid();
        cart.AddItem(entityId, "Vinogradi Sremskih Karlovaca", 300);

        var addition = () => cart.AddItem(entityId, "Vinogradi Sremskih Karlovaca", 300);

        addition.Should().Throw<DomainException>();
    }

    [Fact]
    public void Removing_an_item_that_is_not_in_the_cart_is_rejected()
    {
        var cart = new Cart(WellKnownUsers.Explorer);
        cart.AddItem(Guid.NewGuid(), "Vinogradi Sremskih Karlovaca", 300);

        var removal = () => cart.RemoveItem(Guid.NewGuid());

        removal.Should().Throw<NotFoundException>();
    }

    [Fact]
    public void Empty_cart_cannot_be_purchased()
    {
        var cart = new Cart(WellKnownUsers.Explorer);

        var checkout = () => cart.Checkout();

        checkout.Should().Throw<DomainException>();
    }

    [Fact]
    public void Purchased_cart_is_left_without_items()
    {
        var cart = new Cart(WellKnownUsers.Explorer);
        cart.AddItem(Guid.NewGuid(), "Šetnja tvrđavom", 0);
        cart.AddItem(Guid.NewGuid(), "Vinogradi Sremskih Karlovaca", 300);

        cart.Checkout();

        cart.Items.Should().BeEmpty();
    }
}
