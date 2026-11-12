using Games.Domain.SocialChallenges;

namespace Games.Tests.Integration.Seeds;

internal static class SocialChallengeSeed
{
    public static readonly SocialChallenge DraftSquareGathering = new("Okupljanje na trgu", new DailyTimeWindow(new TimeOnly(18, 0), new TimeOnly(20, 0)), 5, 50, 100, "Svi ste se okupili na trgu!");
    public static readonly SocialChallenge DraftMidnightWalk = new("Ponoćna šetnja", new DailyTimeWindow(new TimeOnly(22, 0), new TimeOnly(2, 0)), 10, 200, 250, "Prošetali ste zajedno kroz ponoć!");

    public static SocialChallenge[] All => [DraftSquareGathering, DraftMidnightWalk];
}
