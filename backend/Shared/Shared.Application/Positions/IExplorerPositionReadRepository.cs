using Shared.Application.ExplorerPositioning;

namespace Shared.Application.Positions;

public interface IExplorerPositionReadRepository
{
    Task<CoordinatesDto?> GetByExplorerIdAsync(Guid explorerId);
}
