using Exploration.Domain.TourExecutions;

namespace Exploration.Application.TourExecutions;

public interface ITourExecutionRepository
{
    Task<TourExecution?> GetActiveByTouristIdAsync(Guid touristId);
    void Add(TourExecution execution);
}
