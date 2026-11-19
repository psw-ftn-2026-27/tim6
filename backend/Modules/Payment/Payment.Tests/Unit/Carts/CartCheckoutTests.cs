using FluentAssertions;
using Payment.Domain.Carts;
using Payment.Domain.Coupons;
using Payment.Domain.GiftCards;
using Payment.Domain.Products;
using Payment.Domain.Wallets;
using Shared.Domain.Exceptions;
using Shared.Tests;
using Xunit;

namespace Payment.Tests.Unit.Carts;

public class CartCheckoutTests
{
    private static readonly DateTime PurchasedAt = new(2026, 11, 1, 12, 0, 0, DateTimeKind.Utc);

    private readonly CartCheckout _cartCheckout = new();

    [Fact]
    public void Coupon_is_applied_before_the_gift_card_and_the_unused_gift_card_amount_is_reported()
    {
        var product = new Product(Guid.NewGuid(), "Vinogradi Sremskih Karlovaca", WellKnownUsers.SecondExplorer, 300);
        var cart = new Cart(WellKnownUsers.Explorer);
        cart.AddItem(product.EntityId, product.Name, product.Price);
        var coupon = new Coupon(product.OwnerId, product.Id, 10, PurchasedAt.AddDays(1), "SAVE0010", PurchasedAt.AddDays(-1));
        var giftCard = new GiftCard(WellKnownUsers.SecondExplorer, cart.UserId, 500, "Srećan put!", PurchasedAt.AddDays(-1));

        var calculation = _cartCheckout.CalculateTotal(cart, [product], coupon, giftCard, PurchasedAt);

        var item = calculation.Items.Should().ContainSingle().Which;
        item.CouponDiscount.Should().Be(30);
        calculation.TotalAfterCoupon.Should().Be(270);
        calculation.GiftCardCoverage.Should().Be(270);
        calculation.WalletCharge.Should().Be(0);
        calculation.GiftCardWaste.Should().Be(230);
    }

    [Fact]
    public void Coupon_for_all_of_an_owners_products_discounts_their_most_expensive_cart_item()
    {
        var ownerId = WellKnownUsers.SecondExplorer;
        var cheaperProduct = new Product(Guid.NewGuid(), "Bački salaši", ownerId, 150);
        var expensiveProduct = new Product(Guid.NewGuid(), "Vinogradi", ownerId, 300);
        var cart = new Cart(WellKnownUsers.Explorer);
        cart.AddItem(cheaperProduct.EntityId, cheaperProduct.Name, cheaperProduct.Price);
        cart.AddItem(expensiveProduct.EntityId, expensiveProduct.Name, expensiveProduct.Price);
        var coupon = new Coupon(ownerId, null, 20, PurchasedAt.AddDays(1), "SAVE0020", PurchasedAt.AddDays(-1));

        var calculation = _cartCheckout.CalculateTotal(cart, [cheaperProduct, expensiveProduct], coupon, null, PurchasedAt);

        calculation.Items.Single(item => item.EntityId == cheaperProduct.EntityId).CouponDiscount.Should().Be(0);
        calculation.Items.Single(item => item.EntityId == expensiveProduct.EntityId).CouponDiscount.Should().Be(60);
    }

    [Fact]
    public void Coupon_for_all_products_uses_cart_prices_when_product_price_has_changed()
    {
        var ownerId = WellKnownUsers.SecondExplorer;
        var changedProduct = new Product(Guid.NewGuid(), "Bački salaši", ownerId, 150);
        var mostExpensiveCartProduct = new Product(Guid.NewGuid(), "Vinogradi", ownerId, 300);
        var cart = new Cart(WellKnownUsers.Explorer);
        cart.AddItem(changedProduct.EntityId, changedProduct.Name, changedProduct.Price);
        cart.AddItem(mostExpensiveCartProduct.EntityId, mostExpensiveCartProduct.Name, mostExpensiveCartProduct.Price);
        changedProduct.PutInOfferAgain(changedProduct.Name, 400);
        var coupon = new Coupon(ownerId, null, 20, PurchasedAt.AddDays(1), "SAVE0020", PurchasedAt.AddDays(-1));

        var calculation = _cartCheckout.CalculateTotal(
            cart,
            [changedProduct, mostExpensiveCartProduct],
            coupon,
            null,
            PurchasedAt);

        calculation.Items.Single(item => item.EntityId == changedProduct.EntityId).CouponDiscount.Should().Be(0);
        calculation.Items.Single(item => item.EntityId == mostExpensiveCartProduct.EntityId).CouponDiscount.Should().Be(60);
    }

    [Fact]
    public void Fractional_coupon_discount_is_rounded_up_to_the_next_whole_ec()
    {
        var product = new Product(Guid.NewGuid(), "Vinogradi", WellKnownUsers.SecondExplorer, 99);
        var cart = new Cart(WellKnownUsers.Explorer);
        cart.AddItem(product.EntityId, product.Name, product.Price);
        var coupon = new Coupon(product.OwnerId, product.Id, 15, PurchasedAt.AddDays(1), "SAVE0015", PurchasedAt.AddDays(-1));

        var calculation = _cartCheckout.CalculateTotal(cart, [product], coupon, null, PurchasedAt);

        calculation.Items.Should().ContainSingle().Which.CouponDiscount.Should().Be(15);
    }

    [Fact]
    public void Product_coupon_without_its_product_in_the_cart_cannot_be_applied()
    {
        var cartProduct = new Product(Guid.NewGuid(), "Bački salaši", WellKnownUsers.SecondExplorer, 150);
        var otherProduct = new Product(Guid.NewGuid(), "Vinogradi", WellKnownUsers.SecondExplorer, 300);
        var cart = new Cart(WellKnownUsers.Explorer);
        cart.AddItem(cartProduct.EntityId, cartProduct.Name, cartProduct.Price);
        var coupon = new Coupon(otherProduct.OwnerId, otherProduct.Id, 20, PurchasedAt.AddDays(1), "SAVE0020", PurchasedAt.AddDays(-1));

        var calculation = () => _cartCheckout.CalculateTotal(cart, [cartProduct, otherProduct], coupon, null, PurchasedAt);

        calculation.Should().Throw<DomainException>();
    }

    [Fact]
    public void Cart_with_a_product_that_is_not_in_the_offer_cannot_be_purchased_and_the_wallet_stays_without_payments()
    {
        var productInOffer = new Product(Guid.NewGuid(), "Vinogradi Sremskih Karlovaca", WellKnownUsers.SecondExplorer, 300);
        var withdrawnProduct = new Product(Guid.NewGuid(), "Bački salaši", WellKnownUsers.SecondExplorer, 150);
        var cart = new Cart(WellKnownUsers.Explorer);
        cart.AddItem(productInOffer.EntityId, productInOffer.Name, productInOffer.Price);
        cart.AddItem(withdrawnProduct.EntityId, withdrawnProduct.Name, withdrawnProduct.Price);
        withdrawnProduct.WithdrawFromOffer();
        var wallet = new Wallet(WellKnownUsers.Explorer);
        wallet.Deposit(WellKnownUsers.Administrator, 1000, PurchasedAt.AddHours(-1));

        var purchase = () => _cartCheckout.Purchase(cart, wallet, [productInOffer, withdrawnProduct], PurchasedAt);

        purchase.Should().Throw<DomainException>();
        wallet.Payments.Should().BeEmpty();
    }
}
