using Games.Domain.ChallengeProgresses;
using Shared.Tests;

namespace Games.Tests.Integration.Seeds;

internal static class ChallengeProgressSeed
{
    public static readonly ChallengeProgress ExplorerHalfwayThroughFortressGates;
    public static readonly ChallengeProgress ExplorerSolvedMuseumStreet;
    public static readonly ChallengeProgress ExplorerSolvedSquareStatues;

    static ChallengeProgressSeed()
    {
        ExplorerHalfwayThroughFortressGates = new(QuizSeed.PublishedFortressGates.Id, WellKnownUsers.Explorer);
        ExplorerHalfwayThroughFortressGates.RecordResult(50, false, new DateTime(2026, 11, 1, 10, 0, 0, DateTimeKind.Utc));

        ExplorerSolvedMuseumStreet = new(QuizSeed.PublishedMuseumStreet.Id, WellKnownUsers.Explorer);
        ExplorerSolvedMuseumStreet.RecordResult(70, true, new DateTime(2026, 11, 2, 10, 0, 0, DateTimeKind.Utc));

        // Goes with the explorer's completed attempt in LocationChallengeAttemptSeed.
        ExplorerSolvedSquareStatues = new(LocationChallengeSeed.PublishedSquareStatues.Id, WellKnownUsers.Explorer);
        ExplorerSolvedSquareStatues.RecordResult(LocationChallengeSeed.PublishedSquareStatues.Xp, true, new DateTime(2026, 11, 3, 10, 0, 0, DateTimeKind.Utc));
    }

    public static ChallengeProgress[] All => [ExplorerHalfwayThroughFortressGates, ExplorerSolvedMuseumStreet, ExplorerSolvedSquareStatues];
}
