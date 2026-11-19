using Exploration.Application.TourExecuting;
using Exploration.Application.TourExecutions;
using Exploration.Application.Tours;
using Microsoft.EntityFrameworkCore;

namespace Exploration.Infrastructure.Persistence.TourExecutions;

internal sealed class TourExecutionReadRepository : ITourExecutionReadRepository
{
    private readonly ExplorationDbContext _dbContext;

    public TourExecutionReadRepository(ExplorationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<List<UnlockedKeyPointDto>> GetUnlockedSecretsAsync(Guid touristId, Guid tourId)
    {
        var touristsExecutions = _dbContext.TourExecutions
            .Where(execution => execution.TouristId == touristId && execution.TourId == tourId);

        return _dbContext.Tours
            .Where(tour => tour.Id == tourId)
            .SelectMany(tour => tour.KeyPoints)
            .Where(keyPoint => touristsExecutions.Any(execution =>
                execution.CompletedKeyPoints.Any(completed => completed.KeyPointId == keyPoint.Id)))
            .OrderBy(keyPoint => keyPoint.Order)
            .Select(keyPoint => new UnlockedKeyPointDto(
                keyPoint.Name,
                new LocationDto(keyPoint.Location.Latitude, keyPoint.Location.Longitude),
                keyPoint.Secret))
            .ToListAsync();
    }
}
