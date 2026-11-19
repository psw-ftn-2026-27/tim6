using Games.Domain.ChallengeProgresses;

namespace Games.Application.ChallengeProgresses;

public interface IChallengeProgressRepository
{
    Task<ChallengeProgress?> GetAsync(Guid challengeId, Guid touristId);
    void Add(ChallengeProgress progress);
}
