using Payment.Domain.Products;
using Shared.Tests;

namespace Payment.Tests.Integration.Seeds;

internal static class ProductSeed
{
    public static readonly Product FreeTourInOffer = new(Guid.Parse("30000000-0000-0000-0000-000000000001"), "Šetnja tvrđavom", WellKnownUsers.Explorer, 0);
    public static readonly Product PaidTourInOffer = new(Guid.Parse("30000000-0000-0000-0000-000000000002"), "Vinogradi Sremskih Karlovaca", WellKnownUsers.Explorer, 300);
    public static readonly Product WithdrawnTour;

    static ProductSeed()
    {
        WithdrawnTour = new(Guid.Parse("30000000-0000-0000-0000-000000000003"), "Bački salaši", WellKnownUsers.SecondExplorer, 150);
        WithdrawnTour.WithdrawFromOffer();
    }

    public static Product[] All => [FreeTourInOffer, PaidTourInOffer, WithdrawnTour];
}
