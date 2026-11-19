using Exploration.Application.TourExecutions;

namespace Exploration.Application.TourExecuting;

public sealed class TourExecutingQueries
{
    private readonly ITourExecutionReadRepository _executionReadRepository;

    public TourExecutingQueries(ITourExecutionReadRepository executionReadRepository)
    {
        _executionReadRepository = executionReadRepository;
    }

    public Task<TourExecutionDto?> GetActiveAsync(Guid touristId, Guid tourId)
    {
        return _executionReadRepository.GetActiveAsync(touristId, tourId);
    }
}
