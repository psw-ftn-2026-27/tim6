using Exploration.Application.TourExecuting;
using Exploration.Application.Tours;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Api;

namespace Exploration.Api;

[ApiController]
[Route("api/exploration/tour-executions")]
[Authorize(Roles = "explorer")]
public sealed class TourExecutingController : ControllerBase
{
    private readonly TourExecutingService _executingService;
    private readonly TourExecutingQueries _executingQueries;

    public TourExecutingController(
        TourExecutingService executingService,
        TourExecutingQueries executingQueries)
    {
        _executingService = executingService;
        _executingQueries = executingQueries;
    }

    [HttpGet("current/{tourId:guid}")]
    public async Task<ActionResult<TourExecutionDto?>> GetCurrent(Guid tourId)
    {
        return await _executingQueries.GetCurrentAsync(User.GetUserId(), tourId);
    }

    [HttpGet("unlocked-secrets/{tourId:guid}")]
    public async Task<ActionResult<List<UnlockedKeyPointDto>>> GetUnlockedSecrets(Guid tourId)
    {
        return await _executingQueries.GetUnlockedSecretsAsync(User.GetUserId(), tourId);
    }

    [HttpPost]
    public async Task<ActionResult> Start(StartTourDto dto)
    {
        await _executingService.StartAsync(User.GetUserId(), dto);
        return NoContent();
    }

    [HttpPost("active/{tourId:guid}/abandon")]
    public async Task<ActionResult> Abandon(Guid tourId)
    {
        await _executingService.AbandonAsync(User.GetUserId(), tourId);
        return NoContent();
    }

    [HttpPost("active/{tourId:guid}/position")]
    public async Task<ActionResult> ReportPosition(Guid tourId, LocationDto position)
    {
        await _executingService.ReportPositionAsync(User.GetUserId(), tourId, position);
        return NoContent();
    }
}
