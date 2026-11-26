using Payment.Domain.Coupons;
using Shared.Tests;

namespace Payment.Tests.Integration.Seeds;

internal static class CouponSeed
{
    public static readonly Coupon ExplorersAllProductsCoupon = new(
        WellKnownUsers.Explorer,
        null,
        10,
        DateTime.UtcNow.AddDays(10),
        DateTime.UtcNow);

    public static readonly Coupon SecondExplorersProductCoupon = new(
        WellKnownUsers.SecondExplorer,
        ProductSeed.WithdrawnTour.Id,
        20,
        DateTime.UtcNow.AddDays(20),
        DateTime.UtcNow);

    public static Coupon[] All => [ExplorersAllProductsCoupon, SecondExplorersProductCoupon];
}
