using Microsoft.EntityFrameworkCore;
using Shared.Application.Positions;
using Shared.Domain.Positions;

namespace Shared.Infrastructure.Persistence.Positions;

internal sealed class ExplorerPositionRepository : IExplorerPositionRepository
{
    private readonly SharedDbContext _dbContext;

    public ExplorerPositionRepository(SharedDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<ExplorerPosition?> GetByExplorerIdAsync(Guid explorerId)
    {
        return _dbContext.ExplorerPositions.FirstOrDefaultAsync(position => position.ExplorerId == explorerId);
    }

    public void Add(ExplorerPosition position)
    {
        _dbContext.ExplorerPositions.Add(position);
    }
}
