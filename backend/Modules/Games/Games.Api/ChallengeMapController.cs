using Games.Application.ChallengeMap;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Api;

namespace Games.Api;

[ApiController]
[Route("api/games/challenge-map")]
[Authorize(Roles = "explorer")]
public sealed class ChallengeMapController : ControllerBase
{
    private readonly ChallengeMapQueries _mapQueries;

    public ChallengeMapController(ChallengeMapQueries mapQueries)
    {
        _mapQueries = mapQueries;
    }

    [HttpGet]
    public async Task<ActionResult<List<ChallengeOnMapDto>>> Get([FromQuery] double? latitude, [FromQuery] double? longitude)
    {
        return await _mapQueries.GetChallengeMapAsync(User.GetUserId(), latitude, longitude);
    }
}
