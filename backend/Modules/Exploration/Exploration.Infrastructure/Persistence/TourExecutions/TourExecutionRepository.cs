using Exploration.Application.TourExecutions;
using Exploration.Domain.TourExecutions;
using Microsoft.EntityFrameworkCore;

namespace Exploration.Infrastructure.Persistence.TourExecutions;

internal sealed class TourExecutionRepository : ITourExecutionRepository
{
    private readonly ExplorationDbContext _dbContext;

    public TourExecutionRepository(ExplorationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<TourExecution?> GetActiveByTouristIdAsync(Guid touristId)
    {
        return _dbContext.TourExecutions.FirstOrDefaultAsync(execution =>
            execution.TouristId == touristId && execution.Status == TourExecutionStatus.Active);
    }

    public void Add(TourExecution execution)
    {
        _dbContext.TourExecutions.Add(execution);
    }
}
