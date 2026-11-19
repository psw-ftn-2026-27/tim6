using Exploration.Domain.TourExecutions;

namespace Exploration.Application.TourExecutions;

public interface ITourExecutionRepository
{
    Task<TourExecution?> GetActiveByTouristIdAsync(Guid touristId);
    Task<TourExecution?> GetLatestAsync(Guid touristId, Guid tourId);
    void Add(TourExecution execution);
}
