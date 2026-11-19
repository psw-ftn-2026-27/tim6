using Games.Application.LocationChallengeAttempts;
using Games.Application.LocationChallenges;
using Shared.Domain;

namespace Games.Application.LocationChallengeSolving;

public sealed class LocationChallengeSolvingQueries
{
    private readonly ILocationChallengeReadRepository _challengeReadRepository;
    private readonly ILocationChallengeAttemptReadRepository _attemptReadRepository;

    public LocationChallengeSolvingQueries(
        ILocationChallengeReadRepository challengeReadRepository,
        ILocationChallengeAttemptReadRepository attemptReadRepository)
    {
        _challengeReadRepository = challengeReadRepository;
        _attemptReadRepository = attemptReadRepository;
    }

    public Task<PageResult<AvailableLocationChallengeDto>> GetAvailableAsync(Guid explorerId, int page, int pageSize)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        return _challengeReadRepository.GetAvailableAsync(explorerId, page, pageSize);
    }

    public Task<LocationChallengeAttemptDto?> GetAttemptAsync(Guid explorerId, Guid challengeId)
    {
        return _attemptReadRepository.GetByChallengeAndExplorerAsync(challengeId, explorerId);
    }
}
