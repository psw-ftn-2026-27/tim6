using FluentAssertions;
using Payment.Domain.Carts;
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
