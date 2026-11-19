using Payment.Domain.Products;
using Shared.Domain.Exceptions;

namespace Payment.Domain.Coupons;

public sealed class CouponIssuer
{
    public Coupon Issue(
        IReadOnlyCollection<Product> ownedProducts,
        CouponIssueDetails details)
    {
        var productsOfOwner = ownedProducts
            .Where(product => product.OwnerId == details.OwnerId)
            .ToList();

        if (productsOfOwner.Count == 0)
        {
            throw new DomainException("A coupon owner must have at least one product.");
        }

        if (details.ProductId is not null && productsOfOwner.All(product => product.Id != details.ProductId))
        {
            throw new DomainException("A coupon can only be created for your own product.");
        }

        var code = Guid.NewGuid().ToString("N")[..8].ToUpperInvariant();
        return new Coupon(
            details.OwnerId,
            details.ProductId,
            details.DiscountPercentage,
            details.ValidUntil,
            code,
            details.CreatedAt);
    }
}
