using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Payment.Application.CouponAuthoring;
using Payment.Infrastructure.Persistence;
using Payment.Tests.Integration.Seeds;
using Shared.Tests;
using Xunit;

namespace Payment.Tests.Integration.CouponAuthoring;

public class CouponAuthoringCommandTests : BaseIntegrationTest
{
    public CouponAuthoringCommandTests(PaymentApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Owner_creates_one_coupon_for_their_product_with_a_unique_eight_character_code()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        using var arrangeContext = Factory.CreateContext<PaymentDbContext>();
        var couponCountBefore = arrangeContext.Coupons.Count();
        var existingCodes = arrangeContext.Coupons.Select(coupon => coupon.Code).ToList();
        var dto = new CreateCouponDto(ProductSeed.PaidTourInOffer.Id, 25, DateTime.UtcNow.AddDays(5));

        var response = await client.PostAsJsonAsync("/api/payment/coupons", dto);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var couponId = await response.Content.ReadFromJsonAsync<Guid>(JsonOptions);
        using var assertContext = Factory.CreateContext<PaymentDbContext>();
        assertContext.Coupons.Should().HaveCount(couponCountBefore + 1);
        var stored = assertContext.Coupons.Single(coupon => coupon.Id == couponId);
        stored.OwnerId.Should().Be(WellKnownUsers.Explorer);
        stored.ProductId.Should().Be(ProductSeed.PaidTourInOffer.Id);
        stored.DiscountPercentage.Should().Be(25);
        stored.Code.Should().HaveLength(8);
        existingCodes.Should().NotContain(stored.Code);
    }

    [Fact]
    public async Task Owner_creates_one_coupon_for_all_their_products()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        using var arrangeContext = Factory.CreateContext<PaymentDbContext>();
        var couponCountBefore = arrangeContext.Coupons.Count();
        var dto = new CreateCouponDto(null, 25, DateTime.UtcNow.AddDays(5));

        var response = await client.PostAsJsonAsync("/api/payment/coupons", dto);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var couponId = await response.Content.ReadFromJsonAsync<Guid>(JsonOptions);
        using var assertContext = Factory.CreateContext<PaymentDbContext>();
        assertContext.Coupons.Should().HaveCount(couponCountBefore + 1);
        var stored = assertContext.Coupons.Single(coupon => coupon.Id == couponId);
        stored.OwnerId.Should().Be(WellKnownUsers.Explorer);
        stored.ProductId.Should().BeNull();
        stored.DiscountPercentage.Should().Be(25);
        stored.Code.Should().HaveLength(8);
    }

    [Fact]
    public async Task Owner_without_products_cannot_create_a_coupon_and_nothing_is_stored()
    {
        var client = Factory.CreateClientFor(Guid.NewGuid(), "explorer");
        using var arrangeContext = Factory.CreateContext<PaymentDbContext>();
        var couponCountBefore = arrangeContext.Coupons.Count();
        var dto = new CreateCouponDto(null, 25, DateTime.UtcNow.AddDays(5));

        var response = await client.PostAsJsonAsync("/api/payment/coupons", dto);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<PaymentDbContext>();
        assertContext.Coupons.Should().HaveCount(couponCountBefore);
    }

    [Fact]
    public async Task Anonymous_user_cannot_create_a_coupon()
    {
        var client = Factory.CreateClient();
        var dto = new CreateCouponDto(null, 25, DateTime.UtcNow.AddDays(5));

        var response = await client.PostAsJsonAsync("/api/payment/coupons", dto);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_create_a_coupon()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var dto = new CreateCouponDto(null, 25, DateTime.UtcNow.AddDays(5));

        var response = await client.PostAsJsonAsync("/api/payment/coupons", dto);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
