using Exploration.Domain.TourExecutions;
using Exploration.Domain.Tours;
using Shared.Tests;

namespace Exploration.Tests.Integration.Seeds;

internal static class TourExecutionSeed
{
    /// <summary>
    /// A fourth user only this module's tests know. <see cref="WellKnownUsers"/> is platform-owned and has
    /// three users, and each of them already plays a role here: Explorer has an active session, and
    /// SecondExplorer must have none so the start tests can start one. Feature modules store users as plain
    /// IDs, so this one needs no identity record.
    /// </summary>
    public static readonly Guid ThirdExplorer = Guid.Parse("00000000-0000-0000-0000-000000000004");

    public static readonly TourExecution ExplorersActiveVineyards = new(WellKnownUsers.Explorer, TourSeed.PublishedVineyards.Id, new Location(45.2050, 19.9350));
    public static readonly TourExecution SecondExplorersAbandonedVineyards;
    public static readonly TourExecution ExplorersAbandonedFortressUndergroundAfterTheFirstKeyPoint;
    public static readonly TourExecution ThirdExplorersActiveFortressUndergroundAfterTwoKeyPoints;
    public static readonly TourExecution SecondExplorersCompletedMonasteries;

    static TourExecutionSeed()
    {
        SecondExplorersAbandonedVineyards = new(WellKnownUsers.SecondExplorer, TourSeed.PublishedVineyards.Id, new Location(45.2040, 19.9340));
        SecondExplorersAbandonedVineyards.Abandon();

        ExplorersAbandonedFortressUndergroundAfterTheFirstKeyPoint = new(WellKnownUsers.Explorer, TourSeed.PublishedFortressUndergroundWithPreview.Id, new Location(45.2530, 19.8620));
        ExplorersAbandonedFortressUndergroundAfterTheFirstKeyPoint.CompleteKeyPoint(TourSeed.PublishedFortressUndergroundWithPreview.KeyPoints[0].Id);
        ExplorersAbandonedFortressUndergroundAfterTheFirstKeyPoint.Abandon();

        ThirdExplorersActiveFortressUndergroundAfterTwoKeyPoints = new(ThirdExplorer, TourSeed.PublishedFortressUndergroundWithPreview.Id, new Location(45.2530, 19.8620));
        ThirdExplorersActiveFortressUndergroundAfterTwoKeyPoints.CompleteKeyPoint(TourSeed.PublishedFortressUndergroundWithPreview.KeyPoints[0].Id);
        ThirdExplorersActiveFortressUndergroundAfterTwoKeyPoints.CompleteKeyPoint(TourSeed.PublishedFortressUndergroundWithPreview.KeyPoints[1].Id);

        SecondExplorersCompletedMonasteries = new(WellKnownUsers.SecondExplorer, TourSeed.PublishedMonasteries.Id, new Location(45.1200, 19.9400));
        SecondExplorersCompletedMonasteries.CompleteKeyPoint(TourSeed.PublishedMonasteries.KeyPoints[0].Id);
        SecondExplorersCompletedMonasteries.CompleteKeyPoint(TourSeed.PublishedMonasteries.KeyPoints[1].Id);
        SecondExplorersCompletedMonasteries.Complete();
    }

    public static TourExecution[] All => [ExplorersActiveVineyards, SecondExplorersAbandonedVineyards, ExplorersAbandonedFortressUndergroundAfterTheFirstKeyPoint, ThirdExplorersActiveFortressUndergroundAfterTwoKeyPoints, SecondExplorersCompletedMonasteries];
}
