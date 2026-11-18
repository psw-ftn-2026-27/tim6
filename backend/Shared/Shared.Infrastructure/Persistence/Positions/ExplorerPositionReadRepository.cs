using Microsoft.EntityFrameworkCore;
using Shared.Application.ExplorerPositioning;
using Shared.Application.Positions;

namespace Shared.Infrastructure.Persistence.Positions;

internal sealed class ExplorerPositionReadRepository : IExplorerPositionReadRepository
{
    private readonly SharedDbContext _dbContext;

    public ExplorerPositionReadRepository(SharedDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<CoordinatesDto?> GetByExplorerIdAsync(Guid explorerId)
    {
        return _dbContext.ExplorerPositions
            .Where(position => position.ExplorerId == explorerId)
            .Select(position => new CoordinatesDto(position.Coordinates.Latitude, position.Coordinates.Longitude))
            .FirstOrDefaultAsync();
    }
}
