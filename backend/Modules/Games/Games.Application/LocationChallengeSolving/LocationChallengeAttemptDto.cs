using Games.Application.Challenges;

namespace Games.Application.LocationChallengeSolving;

public sealed record LocationChallengeAttemptDto(
    Guid Id,
    LocationDto Center,
    double RadiusInMeters,
    List<AttemptHiddenPointDto> HiddenPoints,
    DateTime? CompletedAt,
    int? EarnedXp);
