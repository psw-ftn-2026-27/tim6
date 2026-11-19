using Games.Application.Challenges;

namespace Games.Application.LocationChallengeSolving;

// EarnedXp comes from the explorer's progress on the challenge; it is null until the attempt is completed.
public sealed record LocationChallengeAttemptDto(
    Guid Id,
    LocationDto Center,
    double RadiusInMeters,
    List<AttemptHiddenPointDto> HiddenPoints,
    DateTime? CompletedAt,
    int? EarnedXp);
