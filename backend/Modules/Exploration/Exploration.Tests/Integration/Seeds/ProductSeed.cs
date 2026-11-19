using Payment.Domain.Products;

namespace Exploration.Tests.Integration.Seeds;

internal static class ProductSeed
{
    public static readonly Product VineyardsInOffer = new(TourSeed.PublishedVineyards.Id, TourSeed.PublishedVineyards.Name, TourSeed.PublishedVineyards.AuthorId, 0);
    public static readonly Product MonasteriesInOffer = new(TourSeed.PublishedMonasteries.Id, TourSeed.PublishedMonasteries.Name, TourSeed.PublishedMonasteries.AuthorId, 0);
    public static readonly Product WithdrawnSalashes;
    public static readonly Product FortressUndergroundInOffer = new(TourSeed.PublishedFortressUndergroundWithPreview.Id, TourSeed.PublishedFortressUndergroundWithPreview.Name, TourSeed.PublishedFortressUndergroundWithPreview.AuthorId, 500);
    public static readonly Product PalicInOffer = new(TourSeed.PublishedPalicWithoutPreview.Id, TourSeed.PublishedPalicWithoutPreview.Name, TourSeed.PublishedPalicWithoutPreview.AuthorId, 300);

    static ProductSeed()
    {
        WithdrawnSalashes = new(TourSeed.ArchivedSalashes.Id, TourSeed.ArchivedSalashes.Name, TourSeed.ArchivedSalashes.AuthorId, 150);
        WithdrawnSalashes.WithdrawFromOffer();
    }

    public static Product[] All => [VineyardsInOffer, MonasteriesInOffer, WithdrawnSalashes, FortressUndergroundInOffer, PalicInOffer];
}
