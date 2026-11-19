using Games.Domain.LocationChallengeAttempts;
using Shared.Tests;

namespace Games.Tests.Integration.Seeds;

internal static class LocationChallengeAttemptSeed
{
    public static readonly LocationChallengeAttempt ExplorersOpenBridgeViewsWithFirstPointRevealed;
    public static readonly LocationChallengeAttempt ExplorersCompletedSquareStatues;

    static LocationChallengeAttemptSeed()
    {
        ExplorersOpenBridgeViewsWithFirstPointRevealed = new(LocationChallengeSeed.PublishedBridgeViews.Id, WellKnownUsers.Explorer, 150);
        ExplorersOpenBridgeViewsWithFirstPointRevealed.Reveal(LocationChallengeSeed.PublishedBridgeViews.HiddenPoints[0].Id);

        ExplorersCompletedSquareStatues = new(LocationChallengeSeed.PublishedSquareStatues.Id, WellKnownUsers.Explorer, 90);
        ExplorersCompletedSquareStatues.Reveal(LocationChallengeSeed.PublishedSquareStatues.HiddenPoints[0].Id);
        ExplorersCompletedSquareStatues.Complete();
    }

    public static LocationChallengeAttempt[] All => [ExplorersOpenBridgeViewsWithFirstPointRevealed, ExplorersCompletedSquareStatues];
}
