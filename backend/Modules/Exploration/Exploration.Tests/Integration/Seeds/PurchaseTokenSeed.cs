using Payment.Domain.PurchaseTokens;
using Shared.Tests;

namespace Exploration.Tests.Integration.Seeds;

internal static class PurchaseTokenSeed
{
    private static readonly DateTime PurchaseTime = new(2026, 11, 1, 10, 0, 0, DateTimeKind.Utc);

    public static readonly PurchaseToken ExplorersVineyards = new(WellKnownUsers.Explorer, TourSeed.PublishedVineyards.Id, TourSeed.PublishedVineyards.Name, PurchaseTime);
    public static readonly PurchaseToken SecondExplorersVineyards = new(WellKnownUsers.SecondExplorer, TourSeed.PublishedVineyards.Id, TourSeed.PublishedVineyards.Name, PurchaseTime);
    public static readonly PurchaseToken SecondExplorersMonasteries = new(WellKnownUsers.SecondExplorer, TourSeed.PublishedMonasteries.Id, TourSeed.PublishedMonasteries.Name, PurchaseTime);
    public static readonly PurchaseToken ExplorersFortressUnderground = new(WellKnownUsers.Explorer, TourSeed.PublishedFortressUndergroundWithPreview.Id, TourSeed.PublishedFortressUndergroundWithPreview.Name, PurchaseTime);
    public static readonly PurchaseToken ThirdExplorersFortressUnderground = new(TourExecutionSeed.ThirdExplorer, TourSeed.PublishedFortressUndergroundWithPreview.Id, TourSeed.PublishedFortressUndergroundWithPreview.Name, PurchaseTime);

    public static PurchaseToken[] All => [ExplorersVineyards, SecondExplorersVineyards, SecondExplorersMonasteries, ExplorersFortressUnderground, ThirdExplorersFortressUnderground];
}
