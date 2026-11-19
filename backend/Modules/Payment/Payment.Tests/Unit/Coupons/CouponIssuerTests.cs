using FluentAssertions;
using Payment.Domain.Coupons;
using Payment.Domain.Products;
using Shared.Domain.Exceptions;
using Shared.Tests;
using Xunit;

namespace Payment.Tests.Unit.Coupons;

public class CouponIssuerTests
{
    [Fact]
    public void Coupon_for_one_product_can_only_be_issued_by_its_owner()
    {
        var ownedProduct = new Product(Guid.NewGuid(), "Šetnja tvrđavom", WellKnownUsers.Explorer, 200);
        var anotherOwnersProduct = new Product(Guid.NewGuid(), "Bački salaši", WellKnownUsers.SecondExplorer, 150);
        var details = new CouponIssueDetails(
            WellKnownUsers.Explorer,
            anotherOwnersProduct.Id,
            20,
            DateTime.UtcNow.AddDays(1),
            DateTime.UtcNow);
        var issuer = new CouponIssuer();

        var issue = () => issuer.Issue([ownedProduct], details);

        issue.Should().Throw<DomainException>();
    }

    [Fact]
    public void Owner_without_products_cannot_receive_a_coupon()
    {
        var anotherOwnersProduct = new Product(Guid.NewGuid(), "Bački salaši", WellKnownUsers.SecondExplorer, 150);
        var details = new CouponIssueDetails(
            WellKnownUsers.Explorer,
            null,
            20,
            DateTime.UtcNow.AddDays(1),
            DateTime.UtcNow);
        var issuer = new CouponIssuer();

        var issue = () => issuer.Issue([anotherOwnersProduct], details);

        issue.Should().Throw<DomainException>();
    }
}
