using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Application.ExplorerPositioning;

namespace Shared.Api;

[ApiController]
[Route("api/shared/positions")]
[Authorize(Roles = "explorer")]
public sealed class ExplorerPositioningController : ControllerBase
{
    private readonly ExplorerPositioningService _positioningService;
    private readonly ExplorerPositioningQueries _positioningQueries;

    public ExplorerPositioningController(
        ExplorerPositioningService positioningService,
        ExplorerPositioningQueries positioningQueries)
    {
        _positioningService = positioningService;
        _positioningQueries = positioningQueries;
    }

    [HttpPut("mine")]
    public async Task<ActionResult> Move(CoordinatesDto dto)
    {
        await _positioningService.MoveExplorerAsync(User.GetUserId(), dto);
        return NoContent();
    }

    [HttpGet("mine")]
    public async Task<ActionResult<CoordinatesDto?>> GetMine()
    {
        return await _positioningQueries.GetMyPositionAsync(User.GetUserId());
    }
}
