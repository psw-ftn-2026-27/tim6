using Games.Application.Challenges;

namespace Games.Application.LocationChallengeSolving;

public sealed record AvailableLocationChallengeDto(
    Guid Id,
    string Name,
    LocationDto Location,
    int Xp,
    bool HasAttemptInProgress);
