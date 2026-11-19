using System.Net.Http.Json;
using FluentAssertions;
using Payment.Application.Cart;
using Payment.Infrastructure.Persistence;
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

        cart!.Items.Should().ContainEquivalentOf(new CartItemDto(paidTour.EntityId, paidTour.Name, paidTour.Price, true, 0, paidTour.Price));
        cart.Items.Should().ContainEquivalentOf(new CartItemDto(withdrawnTour.EntityId, withdrawnTour.Name, withdrawnTour.Price, false, 0, withdrawnTour.Price));
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

    [Fact]
    public async Task Buyer_sees_the_current_balance_of_their_wallet_beside_the_cart()
    {
        var client = Factory.CreateClientFor(WalletSeed.ReturningBuyer, "explorer");

        var cart = await client.GetFromJsonAsync<CartDto>("/api/payment/carts/mine", JsonOptions);

        cart!.WalletBalance.Should().Be(WalletSeed.ReturningBuyersWalletWithPayments.Balance());
    }

    [Fact]
    public async Task Buyer_without_a_wallet_sees_a_balance_of_zero_beside_the_cart()
    {
        var client = Factory.CreateClientFor(WalletSeed.WalletlessBuyer, "explorer");

        var cart = await client.GetFromJsonAsync<CartDto>("/api/payment/carts/mine", JsonOptions);

        cart!.WalletBalance.Should().Be(0);
    }

    [Fact]
    public async Task Buyer_previews_item_prices_after_coupon_gift_card_coverage_wallet_charge_and_gift_card_waste()
    {
        var client = Factory.CreateClientFor(WalletSeed.ReturningBuyer, "explorer");
        var product = ProductSeed.PaidTourInOffer;
        var coupon = CouponSeed.ExplorersAllProductsCoupon;
        var giftCard = GiftCardSeed.ValidCardReceivedByReturningBuyer;

        var cart = await client.GetFromJsonAsync<CartDto>(
            $"/api/payment/carts/mine?couponCode={coupon.Code}&giftCardId={giftCard.Id}",
            JsonOptions);

        cart!.Items.Should().ContainSingle().Which.Should().BeEquivalentTo(
            new CartItemDto(product.EntityId, product.Name, product.Price, true, 30, 270));
        cart.TotalAfterCoupon.Should().Be(270);
        cart.GiftCardCoverage.Should().Be(250);
        cart.WalletCharge.Should().Be(20);
        cart.GiftCardWaste.Should().Be(0);
        using var assertContext = Factory.CreateContext<PaymentDbContext>();
        assertContext.Coupons.Single(stored => stored.Id == coupon.Id).RedeemedByUserIds.Should().BeEmpty();
        assertContext.GiftCards.Single(stored => stored.Id == giftCard.Id).IsRedeemed.Should().BeFalse();
    }
}
