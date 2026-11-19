using Exploration.Application.TourExecuting;
using Exploration.Application.TourExecutions;
using Exploration.Application.Tours;
using Exploration.Domain.TourExecutions;
using Microsoft.EntityFrameworkCore;

namespace Exploration.Infrastructure.Persistence.TourExecutions;

internal sealed class TourExecutionReadRepository : ITourExecutionReadRepository
{
    private readonly ExplorationDbContext _dbContext;

    public TourExecutionReadRepository(ExplorationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<TourExecutionDto?> GetActiveAsync(Guid touristId, Guid tourId)
    {
        return _dbContext.TourExecutions
            .Where(execution => execution.TouristId == touristId
                && execution.TourId == tourId
                && execution.Status == TourExecutionStatus.Active)
            .Join(_dbContext.Tours, execution => execution.TourId, tour => tour.Id, (execution, tour) => new TourExecutionDto(
                execution.Id,
                execution.TourId,
                tour.KeyPoints
                    .OrderBy(keyPoint => keyPoint.Order)
                    .Select(keyPoint => new NextKeyPointDto(
                        keyPoint.Name,
                        keyPoint.Description,
                        keyPoint.Image,
                        new LocationDto(keyPoint.Location.Latitude, keyPoint.Location.Longitude)))
                    .First()))
            .FirstOrDefaultAsync();
    }
}
