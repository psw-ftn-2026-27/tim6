using System.Net;
using FluentAssertions;
using Payment.Infrastructure.Persistence;
using Payment.Tests.Integration.Seeds;
using Shared.Tests;
using Xunit;

namespace Payment.Tests.Integration.Cart;

public class CartCommandTests : BaseIntegrationTest
{
    public CartCommandTests(PaymentApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Buyer_without_a_cart_adding_a_product_gets_a_cart_with_an_item_holding_its_name_price_and_entity()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");
        var product = ProductSeed.PaidTourInOffer;
        using var arrangeContext = Factory.CreateContext<PaymentDbContext>();
        var cartCountBefore = arrangeContext.Carts.Count();

        var response = await client.PostAsync($"/api/payment/carts/mine/items/{product.EntityId}", null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<PaymentDbContext>();
        assertContext.Carts.Count().Should().Be(cartCountBefore + 1);
        var stored = assertContext.Carts.Single(cart => cart.UserId == WellKnownUsers.SecondExplorer);
        var item = stored.Items.Should().ContainSingle().Subject;
        item.EntityId.Should().Be(product.EntityId);
        item.Name.Should().Be(product.Name);
        item.Price.Should().Be(product.Price);
    }

    [Fact]
    public async Task Buyer_with_a_cart_adding_a_product_gets_it_in_the_same_cart_and_not_a_second_cart()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var product = ProductSeed.FreeTourInOffer;
        using var arrangeContext = Factory.CreateContext<PaymentDbContext>();
        var cartCountBefore = arrangeContext.Carts.Count();

        var response = await client.PostAsync($"/api/payment/carts/mine/items/{product.EntityId}", null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<PaymentDbContext>();
        assertContext.Carts.Count().Should().Be(cartCountBefore);
        var stored = assertContext.Carts.Single(cart => cart.UserId == WellKnownUsers.Explorer);
        stored.Id.Should().Be(CartSeed.ExplorersCartWithPaidAndWithdrawnTour.Id);
        stored.Items.Should().Contain(item => item.EntityId == product.EntityId);
    }

    [Fact]
    public async Task Adding_a_product_already_in_the_cart_is_rejected_and_the_cart_stays_the_same()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var cartId = CartSeed.ExplorersCartWithPaidAndWithdrawnTour.Id;
        using var arrangeContext = Factory.CreateContext<PaymentDbContext>();
        var itemCountBefore = arrangeContext.Carts.Single(cart => cart.Id == cartId).Items.Count;

        var response = await client.PostAsync($"/api/payment/carts/mine/items/{ProductSeed.PaidTourInOffer.EntityId}", null);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<PaymentDbContext>();
        assertContext.Carts.Single(cart => cart.Id == cartId).Items.Count.Should().Be(itemCountBefore);
    }

    [Fact]
    public async Task Adding_a_product_that_does_not_exist_is_rejected_and_nothing_is_stored()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");
        using var arrangeContext = Factory.CreateContext<PaymentDbContext>();
        var cartCountBefore = arrangeContext.Carts.Count();

        var response = await client.PostAsync($"/api/payment/carts/mine/items/{Guid.NewGuid()}", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<PaymentDbContext>();
        assertContext.Carts.Count().Should().Be(cartCountBefore);
    }

    [Fact]
    public async Task Anonymous_user_cannot_add_a_product_to_a_cart()
    {
        var client = Factory.CreateClient();

        var response = await client.PostAsync($"/api/payment/carts/mine/items/{ProductSeed.FreeTourInOffer.EntityId}", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_add_a_product_to_a_cart()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.PostAsync($"/api/payment/carts/mine/items/{ProductSeed.FreeTourInOffer.EntityId}", null);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Buyer_removing_an_item_keeps_the_cart_and_its_other_items()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var cartId = CartSeed.ExplorersCartWithPaidAndWithdrawnTour.Id;

        var response = await client.DeleteAsync($"/api/payment/carts/mine/items/{ProductSeed.PaidTourInOffer.EntityId}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<PaymentDbContext>();
        var stored = assertContext.Carts.Single(cart => cart.Id == cartId);
        stored.Items.Should().NotContain(item => item.EntityId == ProductSeed.PaidTourInOffer.EntityId);
        stored.Items.Should().Contain(item => item.EntityId == ProductSeed.WithdrawnTour.EntityId);
    }

    [Fact]
    public async Task Removing_an_item_from_a_cart_that_does_not_exist_is_rejected()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");
        using var arrangeContext = Factory.CreateContext<PaymentDbContext>();
        var cartCountBefore = arrangeContext.Carts.Count();

        var response = await client.DeleteAsync($"/api/payment/carts/mine/items/{ProductSeed.PaidTourInOffer.EntityId}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<PaymentDbContext>();
        assertContext.Carts.Count().Should().Be(cartCountBefore);
    }

    [Fact]
    public async Task Anonymous_user_cannot_remove_an_item_from_a_cart()
    {
        var client = Factory.CreateClient();

        var response = await client.DeleteAsync($"/api/payment/carts/mine/items/{ProductSeed.PaidTourInOffer.EntityId}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_remove_an_item_from_a_cart()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.DeleteAsync($"/api/payment/carts/mine/items/{ProductSeed.PaidTourInOffer.EntityId}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
