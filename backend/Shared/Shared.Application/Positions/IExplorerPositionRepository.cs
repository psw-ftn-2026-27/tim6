using Shared.Domain.Positions;

namespace Shared.Application.Positions;

public interface IExplorerPositionRepository
{
    Task<ExplorerPosition?> GetByExplorerIdAsync(Guid explorerId);
    void Add(ExplorerPosition position);
}
