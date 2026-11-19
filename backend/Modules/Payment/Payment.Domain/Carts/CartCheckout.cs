using Payment.Domain.Coupons;
using Payment.Domain.GiftCards;
using Payment.Domain.Products;
using Payment.Domain.PurchaseTokens;
using Payment.Domain.Wallets;
using Shared.Domain.Exceptions;

namespace Payment.Domain.Carts;

public sealed class CartCheckout
{
    public List<PurchaseToken> Purchase(Cart cart, Wallet wallet, IReadOnlyList<Product> products, DateTime purchasedAt)
    {
        return Purchase(cart, wallet, products, null, null, purchasedAt);
    }

    public CartCalculation CalculateTotal(
        Cart cart,
        IReadOnlyList<Product> products,
        Coupon? coupon,
        GiftCard? giftCard,
        DateTime calculatedAt)
    {
        var discountedProduct = FindDiscountedProduct(cart, products, coupon, calculatedAt);
        EnsureGiftCardCanBeUsed(cart, giftCard, calculatedAt);

        var remainingGiftCardAmount = giftCard?.Amount ?? 0;
        var items = cart.Items.Select(item =>
        {
            var couponDiscount = discountedProduct?.EntityId == item.EntityId
                ? (int)Math.Ceiling(item.Price * coupon!.DiscountPercentage / 100m)
                : 0;
            var priceAfterCoupon = item.Price - couponDiscount;
            var coveredByGiftCard = Math.Min(priceAfterCoupon, remainingGiftCardAmount);
            remainingGiftCardAmount -= coveredByGiftCard;

            return new CartItemCalculation(
                item.EntityId,
                item.Name,
                item.Price,
                couponDiscount,
                priceAfterCoupon,
                priceAfterCoupon - coveredByGiftCard,
                discountedProduct?.EntityId == item.EntityId ? coupon!.Id : null);
        }).ToList();

        var totalAfterCoupon = items.Sum(item => item.PriceAfterCoupon);
        var giftCardCoverage = Math.Min(totalAfterCoupon, giftCard?.Amount ?? 0);
        return new CartCalculation(
            items,
            totalAfterCoupon,
            giftCardCoverage,
            totalAfterCoupon - giftCardCoverage,
            Math.Max((giftCard?.Amount ?? 0) - totalAfterCoupon, 0));
    }

    public List<PurchaseToken> Purchase(
        Cart cart,
        Wallet wallet,
        IReadOnlyList<Product> products,
        Coupon? coupon,
        GiftCard? giftCard,
        DateTime purchasedAt)
    {
        var productsInOffer = products.Where(product => product.IsInOffer).ToList();
        if (cart.Items.Any(item => productsInOffer.All(product => product.EntityId != item.EntityId)))
        {
            throw new DomainException("A cart with a product that is not in the offer cannot be purchased.");
        }

        var calculation = CalculateTotal(cart, products, coupon, giftCard, purchasedAt);
        var ownerIds = productsInOffer.ToDictionary(product => product.EntityId, product => product.OwnerId);
        wallet.Pay(calculation.Items, ownerIds, purchasedAt);

        var tokens = calculation.Items
            .Select(item => new PurchaseToken(
                cart.UserId,
                item.EntityId,
                item.Name,
                purchasedAt,
                item.CouponId,
                item.CouponDiscount))
            .ToList();

        coupon?.Redeem(cart.UserId, purchasedAt);
        giftCard?.Redeem(cart.UserId, purchasedAt);
        cart.Checkout();

        return tokens;
    }

    private static Product? FindDiscountedProduct(
        Cart cart,
        IReadOnlyList<Product> products,
        Coupon? coupon,
        DateTime calculatedAt)
    {
        if (coupon is null)
        {
            return null;
        }

        coupon.EnsureCanBeRedeemed(cart.UserId, calculatedAt);
        var productsInCart = products
            .Where(product => cart.Items.Any(item => item.EntityId == product.EntityId))
            .ToList();

        var discountedProduct = coupon.ProductId is null
            ? productsInCart
                .Where(product => product.OwnerId == coupon.OwnerId)
                .OrderByDescending(product => cart.Items.Single(item => item.EntityId == product.EntityId).Price)
                .FirstOrDefault()
            : productsInCart.FirstOrDefault(product => product.Id == coupon.ProductId);

        if (discountedProduct is null)
        {
            throw new DomainException("This coupon can't be applied to any of the items in the cart.");
        }

        return discountedProduct;
    }

    private static void EnsureGiftCardCanBeUsed(Cart cart, GiftCard? giftCard, DateTime calculatedAt)
    {
        giftCard?.EnsureCanBeRedeemed(cart.UserId, calculatedAt);
    }
}
