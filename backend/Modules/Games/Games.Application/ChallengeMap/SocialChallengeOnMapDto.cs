using Games.Application.Challenges;

namespace Games.Application.ChallengeMap;

public sealed record SocialChallengeOnMapDto(
    Guid Id,
    string Name,
    int Xp,
    LocationDto Location,
    int RangeInMeters,
    TimeOnly WindowStart,
    TimeOnly WindowEnd,
    bool IsSolved);
