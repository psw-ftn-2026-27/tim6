using Exploration.Application.TourExecuting;
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

    [HttpGet("active/{tourId:guid}")]
    public async Task<ActionResult<TourExecutionDto?>> GetActive(Guid tourId)
    {
        return await _executingQueries.GetActiveAsync(User.GetUserId(), tourId);
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
}
