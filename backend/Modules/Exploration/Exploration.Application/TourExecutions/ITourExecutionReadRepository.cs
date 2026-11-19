using Exploration.Application.TourExecuting;

namespace Exploration.Application.TourExecutions;

public interface ITourExecutionReadRepository
{
    Task<TourExecutionDto?> GetActiveAsync(Guid touristId, Guid tourId);
}
