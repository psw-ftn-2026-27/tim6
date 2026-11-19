using Games.Application.LocationChallengePublishing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Games.Api;

[ApiController]
[Route("api/games/location-challenges")]
[Authorize(Roles = "administrator")]
public sealed class LocationChallengePublishingController : ControllerBase
{
    private readonly LocationChallengePublishingService _publishingService;

    public LocationChallengePublishingController(LocationChallengePublishingService publishingService)
    {
        _publishingService = publishingService;
    }

    [HttpPost("{id:guid}/publish")]
    public async Task<ActionResult> Publish(Guid id)
    {
        await _publishingService.PublishAsync(id);
        return NoContent();
    }

    [HttpPost("{id:guid}/retire")]
    public async Task<ActionResult> Retire(Guid id)
    {
        await _publishingService.RetireAsync(id);
        return NoContent();
    }

    [HttpPost("{id:guid}/archive")]
    public async Task<ActionResult> Archive(Guid id)
    {
        await _publishingService.ArchiveAsync(id);
        return NoContent();
    }
}
