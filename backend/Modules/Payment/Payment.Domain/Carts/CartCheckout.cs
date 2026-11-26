using Payment.Domain.Coupons;
using Payment.Domain.GiftCards;
using Payment.Domain.Products;
using Payment.Domain.PurchaseTokens;
using Payment.Domain.Wallets;
using Shared.Domain.Exceptions;

namespace Payment.Domain.Carts;

public sealed class CartCheckout
{
    public CartPrice CalculatePrice(
        Cart cart,
        IReadOnlyList<Product> products,
        Coupon? coupon,
        GiftCard? giftCard,
        DateTime calculatedAt)
    {
        EnsureBenefitsCanBeUsed(cart.UserId, coupon, giftCard, calculatedAt);
        var discountedProduct = FindDiscountedProduct(cart, products, coupon);
        var itemPrices = CalculateItemPrices(cart, discountedProduct, coupon, giftCard);
        return CreateCartPrice(itemPrices, giftCard?.Amount ?? 0);
    }

    public List<PurchaseToken> Purchase(
        Cart cart,
        Wallet wallet,
        IReadOnlyList<Product> products,
        Coupon? coupon,
        GiftCard? giftCard,
        DateTime purchasedAt)
    {
        var productsInCart = GetProductsInCart(cart, products);
        EnsureProductsAreInOffer(cart, productsInCart);

        var cartPrice = CalculatePrice(cart, productsInCart, coupon, giftCard, purchasedAt);
        var ownerIds = productsInCart.ToDictionary(product => product.EntityId, product => product.OwnerId);
        wallet.Pay(cartPrice.Items, ownerIds, purchasedAt);

        var tokens = CreatePurchaseTokens(cart.UserId, cartPrice.Items, purchasedAt);

        coupon?.Redeem(cart.UserId, purchasedAt);
        giftCard?.Redeem(cart.UserId, purchasedAt);
        cart.Checkout();

        return tokens;
    }

    private static List<CartItemPrice> CalculateItemPrices(
        Cart cart,
        Product? discountedProduct,
        Coupon? coupon,
        GiftCard? giftCard)
    {
        var prices = new List<CartItemPrice>();
        var remainingGiftCardAmount = giftCard?.Amount ?? 0;

        foreach (var item in cart.Items)
        {
            var price = CalculateItemPrice(item, discountedProduct, coupon, remainingGiftCardAmount);
            prices.Add(price);
            remainingGiftCardAmount -= price.PriceAfterCoupon - price.WalletCharge;
        }

        return prices;
    }

    private static CartItemPrice CalculateItemPrice(
        CartItem item,
        Product? discountedProduct,
        Coupon? coupon,
        int remainingGiftCardAmount)
    {
        var isDiscounted = discountedProduct?.EntityId == item.EntityId;
        var couponDiscount = CalculateCouponDiscount(item.Price, coupon, isDiscounted);

        var priceAfterCoupon = item.Price - couponDiscount;
        var giftCardCoverage = Math.Min(priceAfterCoupon, remainingGiftCardAmount);

        return new CartItemPrice(
            item.EntityId,
            item.Name,
            item.Price,
            couponDiscount,
            priceAfterCoupon,
            priceAfterCoupon - giftCardCoverage,
            isDiscounted && coupon is not null ? coupon.Id : null);
    }

    private static int CalculateCouponDiscount(int price, Coupon? coupon, bool isDiscounted)
    {
        if (!isDiscounted)
        {
            return 0;
        }

        return (int)Math.Ceiling(price * coupon!.DiscountPercentage / 100m);
    }

    private static CartPrice CreateCartPrice(
        IReadOnlyList<CartItemPrice> itemPrices,
        int giftCardAmount)
    {
        var totalAfterCoupon = itemPrices.Sum(item => item.PriceAfterCoupon);
        var giftCardCoverage = Math.Min(totalAfterCoupon, giftCardAmount);

        return new CartPrice(
            itemPrices,
            totalAfterCoupon,
            giftCardCoverage,
            totalAfterCoupon - giftCardCoverage,
            Math.Max(giftCardAmount - totalAfterCoupon, 0));
    }

    private static void EnsureBenefitsCanBeUsed(
        Guid userId,
        Coupon? coupon,
        GiftCard? giftCard,
        DateTime calculatedAt)
    {
        coupon?.EnsureCanBeRedeemed(userId, calculatedAt);
        giftCard?.EnsureCanBeRedeemed(userId, calculatedAt);
    }

    private static List<Product> GetProductsInCart(Cart cart, IEnumerable<Product> products)
    {
        return products
            .Where(product => cart.Items.Any(item => item.EntityId == product.EntityId))
            .ToList();
    }

    private static void EnsureProductsAreInOffer(Cart cart, IReadOnlyList<Product> productsInCart)
    {
        var allProductsAreInOffer = cart.Items.All(item =>
            productsInCart.Any(product => product.EntityId == item.EntityId && product.IsInOffer));

        if (!allProductsAreInOffer)
            throw new DomainException("A cart with a product that is not in the offer cannot be purchased.");
    }

    private static List<PurchaseToken> CreatePurchaseTokens(
        Guid userId,
        IReadOnlyList<CartItemPrice> itemPrices,
        DateTime purchasedAt)
    {
        return itemPrices
            .Select(item => new PurchaseToken(
                userId,
                item.EntityId,
                item.Name,
                purchasedAt,
                item.CouponId,
                item.CouponDiscount))
            .ToList();
    }

    private static Product? FindDiscountedProduct(
        Cart cart,
        IReadOnlyList<Product> products,
        Coupon? coupon)
    {
        if (coupon is null) return null;

        var applicableProducts = GetProductsInCart(cart, products)
            .Where(coupon.AppliesTo);

        var discountedProduct = coupon.ProductId is null
            ? FindMostExpensiveProduct(cart, applicableProducts)
            : applicableProducts.FirstOrDefault();

        return discountedProduct
            ?? throw new DomainException("This coupon can't be applied to any of the items in the cart.");
    }

    private static Product? FindMostExpensiveProduct(Cart cart, IEnumerable<Product> products)
    {
        return products
            .OrderByDescending(product => cart.Items.Single(item => item.EntityId == product.EntityId).Price)
            .FirstOrDefault();
    }
}
