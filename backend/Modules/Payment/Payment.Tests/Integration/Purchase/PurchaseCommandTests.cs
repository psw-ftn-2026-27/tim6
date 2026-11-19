using System.Net;
using FluentAssertions;
using Payment.Infrastructure.Persistence;
using Payment.Tests.Integration.Seeds;
using Shared.Tests;
using Xunit;

namespace Payment.Tests.Integration.Purchase;

public class PurchaseCommandTests : BaseIntegrationTest
{
    public PurchaseCommandTests(PaymentApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Buyer_purchasing_their_cart_gets_a_payment_and_a_purchase_token_for_each_item_and_an_empty_cart()
    {
        var client = Factory.CreateClientFor(WalletSeed.ReturningBuyer, "explorer");
        var product = ProductSeed.PaidTourInOffer;
        using var arrangeContext = Factory.CreateContext<PaymentDbContext>();
        var tokenCountBefore = arrangeContext.PurchaseTokens.Count();
        var paymentCountBefore = arrangeContext.Wallets
            .Single(wallet => wallet.Id == WalletSeed.ReturningBuyersWalletWithPayments.Id).Payments.Count;

        var response = await client.PostAsync("/api/payment/purchases/mine", null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<PaymentDbContext>();
        var wallet = assertContext.Wallets.Single(wallet => wallet.Id == WalletSeed.ReturningBuyersWalletWithPayments.Id);
        wallet.Payments.Should().HaveCount(paymentCountBefore + 1);
        var payment = wallet.Payments.Single(payment => payment.EntityId == product.EntityId);
        payment.OwnerId.Should().Be(product.OwnerId);
        payment.Price.Should().Be(product.Price);
        payment.PaidAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
        assertContext.PurchaseTokens.Count().Should().Be(tokenCountBefore + 1);
        var token = assertContext.PurchaseTokens.Single(token => token.UserId == WalletSeed.ReturningBuyer && token.EntityId == product.EntityId);
        token.Name.Should().Be(product.Name);
        token.PurchasedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
        assertContext.Carts.Single(cart => cart.Id == CartSeed.ReturningBuyersCartWithPaidTour.Id).Items.Should().BeEmpty();
    }

    [Fact]
    public async Task Purchase_without_enough_EC_is_rejected_and_no_payment_or_token_is_stored_and_the_cart_stays_the_same()
    {
        var client = Factory.CreateClientFor(WalletSeed.UnderfundedBuyer, "explorer");
        using var arrangeContext = Factory.CreateContext<PaymentDbContext>();
        var tokenCountBefore = arrangeContext.PurchaseTokens.Count();
        var paymentCountBefore = arrangeContext.Wallets
            .Single(wallet => wallet.Id == WalletSeed.UnderfundedBuyersWallet.Id).Payments.Count;
        var itemCountBefore = arrangeContext.Carts
            .Single(cart => cart.Id == CartSeed.UnderfundedBuyersCartWithPaidTour.Id).Items.Count;

        var response = await client.PostAsync("/api/payment/purchases/mine", null);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<PaymentDbContext>();
        assertContext.Wallets.Single(wallet => wallet.Id == WalletSeed.UnderfundedBuyersWallet.Id)
            .Payments.Count.Should().Be(paymentCountBefore);
        assertContext.PurchaseTokens.Count().Should().Be(tokenCountBefore);
        assertContext.Carts.Single(cart => cart.Id == CartSeed.UnderfundedBuyersCartWithPaidTour.Id)
            .Items.Count.Should().Be(itemCountBefore);
    }

    [Fact]
    public async Task Purchase_by_a_buyer_without_a_cart_is_rejected_as_a_cart_that_does_not_exist()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");
        using var arrangeContext = Factory.CreateContext<PaymentDbContext>();
        var tokenCountBefore = arrangeContext.PurchaseTokens.Count();
        var walletCountBefore = arrangeContext.Wallets.Count();

        var response = await client.PostAsync("/api/payment/purchases/mine", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<PaymentDbContext>();
        assertContext.PurchaseTokens.Count().Should().Be(tokenCountBefore);
        assertContext.Wallets.Count().Should().Be(walletCountBefore);
    }

    [Fact]
    public async Task Buyer_without_a_wallet_purchasing_a_free_tour_gets_a_purchase_token()
    {
        var client = Factory.CreateClientFor(WalletSeed.WalletlessBuyer, "explorer");
        var product = ProductSeed.FreeTourInOffer;
        using var arrangeContext = Factory.CreateContext<PaymentDbContext>();
        var tokenCountBefore = arrangeContext.PurchaseTokens.Count();

        var response = await client.PostAsync("/api/payment/purchases/mine", null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<PaymentDbContext>();
        assertContext.PurchaseTokens.Count().Should().Be(tokenCountBefore + 1);
        assertContext.PurchaseTokens.Should().ContainSingle(token => token.UserId == WalletSeed.WalletlessBuyer && token.EntityId == product.EntityId);
        assertContext.Wallets.Should().ContainSingle(wallet => wallet.UserId == WalletSeed.WalletlessBuyer);
    }

    [Fact]
    public async Task Anonymous_user_cannot_purchase_a_cart()
    {
        var client = Factory.CreateClient();

        var response = await client.PostAsync("/api/payment/purchases/mine", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_purchase_a_cart()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.PostAsync("/api/payment/purchases/mine", null);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Purchase_with_benefits_charges_only_the_uncovered_amount_and_redeems_and_records_the_benefits()
    {
        var client = Factory.CreateClientFor(WalletSeed.ReturningBuyer, "explorer");
        var coupon = CouponSeed.ExplorersAllProductsCoupon;
        var giftCard = GiftCardSeed.ValidCardReceivedByReturningBuyer;
        var product = ProductSeed.PaidTourInOffer;
        using var arrangeContext = Factory.CreateContext<PaymentDbContext>();
        var paymentCountBefore = arrangeContext.Wallets
            .Single(wallet => wallet.Id == WalletSeed.ReturningBuyersWalletWithPayments.Id).Payments.Count;

        var response = await client.PostAsync(
            $"/api/payment/purchases/mine?couponCode={coupon.Code}&giftCardId={giftCard.Id}",
            null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<PaymentDbContext>();
        var wallet = assertContext.Wallets.Single(wallet => wallet.Id == WalletSeed.ReturningBuyersWalletWithPayments.Id);
        wallet.Payments.Should().HaveCount(paymentCountBefore + 1);
        wallet.Payments.Single(payment => payment.EntityId == product.EntityId).Price.Should().Be(20);
        assertContext.Carts.Single(cart => cart.Id == CartSeed.ReturningBuyersCartWithPaidTour.Id).Items.Should().BeEmpty();
        var token = assertContext.PurchaseTokens.Single(token =>
            token.UserId == WalletSeed.ReturningBuyer && token.EntityId == product.EntityId);
        token.CouponId.Should().Be(coupon.Id);
        token.CouponDiscount.Should().Be(30);
        assertContext.Coupons.Single(stored => stored.Id == coupon.Id).RedeemedByUserIds
            .Should().Contain(WalletSeed.ReturningBuyer);
        var storedGiftCard = assertContext.GiftCards.Single(stored => stored.Id == giftCard.Id);
        storedGiftCard.IsRedeemed.Should().BeTrue();
        storedGiftCard.RedeemedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task Rejected_purchase_keeps_wallet_cart_tokens_coupon_and_gift_card_unchanged()
    {
        var client = Factory.CreateClientFor(WalletSeed.UnderfundedBuyer, "explorer");
        var coupon = CouponSeed.ExplorersAllProductsCoupon;
        var foreignGiftCard = GiftCardSeed.ValidCardReceivedByReturningBuyer;
        using var arrangeContext = Factory.CreateContext<PaymentDbContext>();
        var tokenCountBefore = arrangeContext.PurchaseTokens.Count();
        var paymentCountBefore = arrangeContext.Wallets
            .Single(wallet => wallet.Id == WalletSeed.UnderfundedBuyersWallet.Id).Payments.Count;
        var itemCountBefore = arrangeContext.Carts
            .Single(cart => cart.Id == CartSeed.UnderfundedBuyersCartWithPaidTour.Id).Items.Count;

        var response = await client.PostAsync(
            $"/api/payment/purchases/mine?couponCode={coupon.Code}&giftCardId={foreignGiftCard.Id}",
            null);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<PaymentDbContext>();
        assertContext.Wallets.Single(wallet => wallet.Id == WalletSeed.UnderfundedBuyersWallet.Id)
            .Payments.Should().HaveCount(paymentCountBefore);
        assertContext.Carts.Single(cart => cart.Id == CartSeed.UnderfundedBuyersCartWithPaidTour.Id)
            .Items.Should().HaveCount(itemCountBefore);
        assertContext.PurchaseTokens.Should().HaveCount(tokenCountBefore);
        assertContext.Coupons.Single(stored => stored.Id == coupon.Id).RedeemedByUserIds.Should().BeEmpty();
        assertContext.GiftCards.Single(stored => stored.Id == foreignGiftCard.Id).IsRedeemed.Should().BeFalse();
    }
}
