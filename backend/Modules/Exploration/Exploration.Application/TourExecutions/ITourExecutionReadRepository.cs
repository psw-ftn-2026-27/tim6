using Exploration.Application.TourExecuting;

namespace Exploration.Application.TourExecutions;

public interface ITourExecutionReadRepository
{
    Task<List<UnlockedKeyPointDto>> GetUnlockedSecretsAsync(Guid touristId, Guid tourId);
}
