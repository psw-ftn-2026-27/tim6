using Exploration.Domain.TourExecutions;
using Exploration.Domain.Tours;
using Shared.Tests;

namespace Exploration.Tests.Integration.Seeds;

internal static class TourExecutionSeed
{
    public static readonly TourExecution ExplorersActiveVineyards = new(WellKnownUsers.Explorer, TourSeed.PublishedVineyards.Id, new Location(45.2050, 19.9350));
    public static readonly TourExecution SecondExplorersAbandonedVineyards;

    static TourExecutionSeed()
    {
        SecondExplorersAbandonedVineyards = new(WellKnownUsers.SecondExplorer, TourSeed.PublishedVineyards.Id, new Location(45.2040, 19.9340));
        SecondExplorersAbandonedVineyards.Abandon();
    }

    public static TourExecution[] All => [ExplorersActiveVineyards, SecondExplorersAbandonedVineyards];
}
