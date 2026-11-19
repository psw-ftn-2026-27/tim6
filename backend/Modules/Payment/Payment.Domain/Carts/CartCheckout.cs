using Payment.Domain.Products;
using Payment.Domain.PurchaseTokens;
using Payment.Domain.Wallets;
using Shared.Domain.Exceptions;

namespace Payment.Domain.Carts;

public sealed class CartCheckout
{
    public List<PurchaseToken> Purchase(Cart cart, Wallet wallet, IReadOnlyList<Product> products, DateTime purchasedAt)
    {
        var productsInOffer = products.Where(product => product.IsInOffer).ToList();
        if (cart.Items.Any(item => productsInOffer.All(product => product.EntityId != item.EntityId)))
        {
            throw new DomainException("A cart with a product that is not in the offer cannot be purchased.");
        }

        var ownerIds = productsInOffer.ToDictionary(product => product.EntityId, product => product.OwnerId);
        wallet.Pay(cart.Items, ownerIds, purchasedAt);

        var tokens = cart.Items
            .Select(item => new PurchaseToken(cart.UserId, item.EntityId, item.Name, purchasedAt))
            .ToList();
        cart.Checkout();

        return tokens;
    }
}
