using Games.Application.Challenges;

namespace Games.Application.ChallengeMap;

// DistanceInMeters is null when the tourist has not marked a position.
public sealed record ChallengeOnMapDto(
    Guid Id,
    ChallengeKind Kind,
    string Name,
    int Xp,
    LocationDto Location,
    int ActivationRadiusInMeters,
    bool IsSolved,
    double? DistanceInMeters,
    bool CanActivate);
