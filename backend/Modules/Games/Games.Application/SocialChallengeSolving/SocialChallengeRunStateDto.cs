using Games.Application.Challenges;

namespace Games.Application.SocialChallengeSolving;

// EarnedXp and ExitMessage are set only for a tourist who solved the challenge.
public sealed record SocialChallengeRunStateDto(
    Guid Id,
    string Name,
    LocationDto? Location,
    int RangeInMeters,
    SolvingStatus Status,
    int ActiveCount,
    int RequiredPeople,
    int? EarnedXp,
    string? ExitMessage);
