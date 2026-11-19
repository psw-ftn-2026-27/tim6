using Games.Application.LocationChallengeAuthoring;
using Games.Application.LocationChallengeSolving;
using Shared.Domain;

namespace Games.Application.LocationChallenges;

public interface ILocationChallengeReadRepository
{
    Task<List<LocationChallengeDto>> GetAllAsync();
    Task<PageResult<AvailableLocationChallengeDto>> GetAvailableAsync(Guid explorerId, int page, int pageSize);
}
