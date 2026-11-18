using System.Net.Http.Json;
using FluentAssertions;
using Payment.Application.Cart;
using Payment.Tests.Integration.Seeds;
using Shared.Tests;
using Xunit;

namespace Payment.Tests.Integration.Cart;

public class CartQueryTests : BaseIntegrationTest
{
    public CartQueryTests(PaymentApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Buyer_sees_their_items_with_name_and_price_the_withdrawn_product_marked_unavailable_and_the_total_price()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var paidTour = ProductSeed.PaidTourInOffer;
        var withdrawnTour = ProductSeed.WithdrawnTour;

        var cart = await client.GetFromJsonAsync<CartDto>("/api/payment/carts/mine", JsonOptions);

        cart!.Items.Should().ContainEquivalentOf(new CartItemDto(paidTour.EntityId, paidTour.Name, paidTour.Price, true));
        cart.Items.Should().ContainEquivalentOf(new CartItemDto(withdrawnTour.EntityId, withdrawnTour.Name, withdrawnTour.Price, false));
        cart.Items.Should().NotContain(item => item.EntityId == ProductSeed.FreeTourInOffer.EntityId);
        cart.TotalPrice.Should().Be(CartSeed.ExplorersCartWithPaidAndWithdrawnTour.TotalPrice());
    }

    [Fact]
    public async Task Buyer_without_a_cart_sees_an_empty_cart_with_a_total_of_zero()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");

        var cart = await client.GetFromJsonAsync<CartDto>("/api/payment/carts/mine", JsonOptions);

        cart!.Items.Should().BeEmpty();
        cart.TotalPrice.Should().Be(0);
    }
}
