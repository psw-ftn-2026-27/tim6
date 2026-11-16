using Shared.Application.Positions;

namespace Shared.Application.ExplorerPositioning;

public sealed class ExplorerPositioningQueries
{
    private readonly IExplorerPositionReadRepository _positionReadRepository;

    public ExplorerPositioningQueries(IExplorerPositionReadRepository positionReadRepository)
    {
        _positionReadRepository = positionReadRepository;
    }

    public Task<CoordinatesDto?> GetMyPositionAsync(Guid explorerId)
    {
        return _positionReadRepository.GetByExplorerIdAsync(explorerId);
    }
}
