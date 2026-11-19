using Games.Application.LocationChallengeSolving;

namespace Games.Application.LocationChallengeAttempts;

public interface ILocationChallengeAttemptReadRepository
{
    Task<LocationChallengeAttemptDto?> GetByChallengeAndExplorerAsync(Guid challengeId, Guid explorerId);
}
