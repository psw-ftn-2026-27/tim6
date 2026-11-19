using Games.Application.LocationChallengeAuthoring;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Games.Api;

[ApiController]
[Route("api/games/location-challenges")]
[Authorize(Roles = "administrator")]
public sealed class LocationChallengeAuthoringController : ControllerBase
{
    private readonly LocationChallengeAuthoringService _authoringService;
    private readonly LocationChallengeAuthoringQueries _authoringQueries;

    public LocationChallengeAuthoringController(
        LocationChallengeAuthoringService authoringService,
        LocationChallengeAuthoringQueries authoringQueries)
    {
        _authoringService = authoringService;
        _authoringQueries = authoringQueries;
    }

    [HttpGet]
    public async Task<ActionResult<List<LocationChallengeDto>>> GetAll()
    {
        return await _authoringQueries.GetAllAsync();
    }

    [HttpPost]
    public async Task<ActionResult<Guid>> Create(CreateLocationChallengeDto dto)
    {
        return await _authoringService.CreateAsync(dto);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult> Edit(Guid id, UpdateLocationChallengeDto dto)
    {
        await _authoringService.EditAsync(id, dto);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        await _authoringService.DeleteAsync(id);
        return NoContent();
    }
}
