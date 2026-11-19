using Games.Application.Challenges;

namespace Games.Application.LocationChallengeSolving;

// A challenge is not started, in progress (HasAttemptInProgress) or solved (IsSolved).
public sealed record AvailableLocationChallengeDto(
    Guid Id,
    string Name,
    LocationDto Location,
    int Xp,
    bool HasAttemptInProgress,
    bool IsSolved);
