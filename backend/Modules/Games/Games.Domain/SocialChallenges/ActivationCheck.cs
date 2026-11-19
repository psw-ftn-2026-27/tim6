using Games.Domain.Challenges;

namespace Games.Domain.SocialChallenges;

// What deciding whether a tourist can activate a social challenge needs: the challenge as the read
// use case knows it, without loading it, and the tourist's position and moment, in UTC.
public sealed record ActivationCheck(
    Location Location,
    int RangeInMeters,
    DailyTimeWindow TimeWindow,
    bool IsSolved,
    Location? Position,
    DateTime Moment);
