using Payment.Domain.Products;
using Shared.Domain.Exceptions;

namespace Payment.Domain.Coupons;

public sealed class CouponIssuer
{
    public Coupon Issue(
        IReadOnlyCollection<Product> ownedProducts,
        CouponIssueDetails details)
    {
        EnsureCanBeIssued(ownedProducts, details);

        return new Coupon(
            details.OwnerId,
            details.ProductId,
            details.DiscountPercentage,
            details.ExpiresAt,
            details.CreatedAt
        );
    }

    private static void EnsureCanBeIssued(
        IReadOnlyCollection<Product> products,
        CouponIssueDetails details)
    {
        var ownedProducts = products.Where(product => product.OwnerId == details.OwnerId).ToList();
        if (ownedProducts.Count == 0)
            throw new DomainException("Create at least one product before creating a coupon.");

        if (details.ProductId is not null && ownedProducts.All(product => product.Id != details.ProductId))
            throw new DomainException("You can only create coupons for your own products.");
    }
}
