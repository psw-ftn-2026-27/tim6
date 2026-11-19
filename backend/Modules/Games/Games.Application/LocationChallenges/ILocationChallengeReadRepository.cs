using Games.Application.LocationChallengeAuthoring;

namespace Games.Application.LocationChallenges;

public interface ILocationChallengeReadRepository
{
    Task<List<LocationChallengeDto>> GetAllAsync();
}
