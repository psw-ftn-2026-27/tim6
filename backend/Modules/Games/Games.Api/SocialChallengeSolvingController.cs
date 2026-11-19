using Games.Application.Challenges;
using Games.Application.SocialChallengeSolving;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Api;

namespace Games.Api;

[ApiController]
[Route("api/games/social-challenges")]
[Authorize(Roles = "explorer")]
public sealed class SocialChallengeSolvingController : ControllerBase
{
    private readonly SocialChallengeSolvingService _solvingService;
    private readonly SocialChallengeSolvingQueries _solvingQueries;

    public SocialChallengeSolvingController(SocialChallengeSolvingService solvingService,
        SocialChallengeSolvingQueries solvingQueries)
    {
        _solvingService = solvingService;
        _solvingQueries = solvingQueries;
    }

    [HttpPost("{id:guid}/activate")]
    public async Task<ActionResult> Activate(Guid id, LocationDto position)
    {
        await _solvingService.ActivateAsync(id, User.GetUserId(), position);
        return NoContent();
    }

    [HttpPost("{id:guid}/check-position")]
    public async Task<ActionResult> CheckPosition(Guid id, LocationDto position)
    {
        await _solvingService.CheckPositionAsync(id, User.GetUserId(), position);
        return NoContent();
    }

    [HttpGet("{id:guid}/run-state")]
    public async Task<ActionResult<SocialChallengeRunStateDto>> GetRunState(Guid id)
    {
        return await _solvingQueries.GetRunStateAsync(id, User.GetUserId());
    }
}
