using Games.Domain.LocationChallengeAttempts;

namespace Games.Application.LocationChallengeAttempts;

public interface ILocationChallengeAttemptRepository
{
    Task<LocationChallengeAttempt?> GetByChallengeAndExplorerAsync(Guid challengeId, Guid explorerId);
    void Add(LocationChallengeAttempt attempt);
}
