using Games.Application.LocationChallenges;

namespace Games.Application.LocationChallengeAuthoring;

public sealed class LocationChallengeAuthoringQueries
{
    private readonly ILocationChallengeReadRepository _challengeReadRepository;

    public LocationChallengeAuthoringQueries(ILocationChallengeReadRepository challengeReadRepository)
    {
        _challengeReadRepository = challengeReadRepository;
    }

    public Task<List<LocationChallengeDto>> GetAllAsync()
    {
        return _challengeReadRepository.GetAllAsync();
    }
}
