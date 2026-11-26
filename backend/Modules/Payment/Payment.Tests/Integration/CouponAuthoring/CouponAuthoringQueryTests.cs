using System.Net.Http.Json;
using FluentAssertions;
using Payment.Application.CouponAuthoring;
using Payment.Application.Coupons;
using Payment.Application.Products;
using Payment.Tests.Integration.Seeds;
using Shared.Domain;
using Shared.Tests;
using Xunit;

namespace Payment.Tests.Integration.CouponAuthoring;

public class CouponAuthoringQueryTests : BaseIntegrationTest
{
    public CouponAuthoringQueryTests(PaymentApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Product_selection_shows_only_products_owned_by_the_signed_in_owner()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var products = await client.GetFromJsonAsync<List<ProductDto>>(
            "/api/payment/coupons/products", JsonOptions);

        products.Should().Contain(product => product.Id == ProductSeed.FreeTourInOffer.Id);
        products.Should().Contain(product => product.Id == ProductSeed.PaidTourInOffer.Id);
        products.Should().NotContain(product => product.Id == ProductSeed.WithdrawnTour.Id);
    }

    [Fact]
    public async Task Coupon_list_shows_only_coupons_owned_by_the_signed_in_owner()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var coupons = await client.GetFromJsonAsync<PageResult<CouponDto>>(
            "/api/payment/coupons?page=1&pageSize=20", JsonOptions);

        coupons!.Items.Should().Contain(coupon => coupon.Id == CouponSeed.ExplorersAllProductsCoupon.Id);
        coupons.Items.Should().NotContain(coupon => coupon.Id == CouponSeed.SecondExplorersProductCoupon.Id);
    }
}
