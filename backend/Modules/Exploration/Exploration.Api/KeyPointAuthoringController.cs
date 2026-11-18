using Exploration.Application.KeyPointAuthoring;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Api;

namespace Exploration.Api;

[ApiController]
[Route("api/exploration/tours")]
[Authorize(Roles = "explorer")]
public sealed class KeyPointAuthoringController : ControllerBase
{
    private readonly KeyPointAuthoringService _keyPointService;
    private readonly KeyPointAuthoringQueries _keyPointQueries;

    public KeyPointAuthoringController(
        KeyPointAuthoringService keyPointService,
        KeyPointAuthoringQueries keyPointQueries)
    {
        _keyPointService = keyPointService;
        _keyPointQueries = keyPointQueries;
    }

    [HttpGet("{id:guid}/key-points")]
    public async Task<ActionResult<TourKeyPointsDto>> GetKeyPoints(Guid id)
    {
        return await _keyPointQueries.GetKeyPointsAsync(id, User.GetUserId());
    }

    [HttpPost("{id:guid}/key-points")]
    public async Task<ActionResult> AddKeyPoint(Guid id, SaveKeyPointDto dto)
    {
        await _keyPointService.AddKeyPointAsync(id, User.GetUserId(), dto);
        return NoContent();
    }

    [HttpPut("{id:guid}/key-points/{keyPointId:guid}")]
    public async Task<ActionResult> UpdateKeyPoint(Guid id, Guid keyPointId, SaveKeyPointDto dto)
    {
        await _keyPointService.UpdateKeyPointAsync(id, keyPointId, User.GetUserId(), dto);
        return NoContent();
    }

    [HttpDelete("{id:guid}/key-points/{keyPointId:guid}")]
    public async Task<ActionResult> RemoveKeyPoint(Guid id, Guid keyPointId, RemoveKeyPointDto dto)
    {
        await _keyPointService.RemoveKeyPointAsync(id, keyPointId, User.GetUserId(), dto);
        return NoContent();
    }
}
