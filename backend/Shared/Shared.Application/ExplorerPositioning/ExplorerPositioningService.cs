using Shared.Application.Positions;
using Shared.Domain.Positions;

namespace Shared.Application.ExplorerPositioning;

public sealed class ExplorerPositioningService
{
    private readonly IExplorerPositionRepository _positionRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ExplorerPositioningService(IExplorerPositionRepository positionRepository, IUnitOfWork unitOfWork)
    {
        _positionRepository = positionRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task MoveExplorerAsync(Guid explorerId, CoordinatesDto dto)
    {
        var coordinates = new Coordinates(dto.Latitude, dto.Longitude);
        var position = await _positionRepository.GetByExplorerIdAsync(explorerId);
        if (position is null)
        {
            _positionRepository.Add(new ExplorerPosition(explorerId, coordinates));
        }
        else
        {
            position.MoveTo(coordinates);
        }

        await _unitOfWork.SaveChangesAsync();
    }
}
