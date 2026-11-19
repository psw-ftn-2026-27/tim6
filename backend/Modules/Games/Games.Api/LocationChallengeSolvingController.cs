using Games.Application.Challenges;
using Games.Application.LocationChallengeSolving;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Api;
using Shared.Domain;

namespace Games.Api;

[ApiController]
[Route("api/games/location-challenges")]
[Authorize(Roles = "explorer")]
public sealed class LocationChallengeSolvingController : ControllerBase
{
    private readonly LocationChallengeSolvingService _solvingService;
    private readonly LocationChallengeSolvingQueries _solvingQueries;

    public LocationChallengeSolvingController(
        LocationChallengeSolvingService solvingService,
        LocationChallengeSolvingQueries solvingQueries)
    {
        _solvingService = solvingService;
        _solvingQueries = solvingQueries;
    }

    [HttpGet("available")]
    public async Task<ActionResult<PageResult<AvailableLocationChallengeDto>>> GetAvailable(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        return await _solvingQueries.GetAvailableAsync(User.GetUserId(), page, pageSize);
    }

    [HttpGet("{id:guid}/attempt")]
    public async Task<ActionResult<LocationChallengeAttemptDto?>> GetAttempt(Guid id)
    {
        return await _solvingQueries.GetAttemptAsync(User.GetUserId(), id);
    }

    [HttpPost("{id:guid}/activate")]
    public async Task<ActionResult<Guid>> Activate(Guid id, LocationDto position)
    {
        return await _solvingService.ActivateAsync(User.GetUserId(), id, position);
    }

    [HttpPost("{id:guid}/check-location")]
    public async Task<ActionResult> CheckLocation(Guid id, LocationDto position)
    {
        await _solvingService.CheckLocationAsync(User.GetUserId(), id, position);
        return NoContent();
    }
}
