using Games.Application.SocialChallengePublishing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Games.Api;

[ApiController]
[Route("api/games/social-challenges")]
[Authorize(Roles = "administrator")]
public sealed class SocialChallengePublishingController : ControllerBase
{
    private readonly SocialChallengePublishingService _publishingService;

    public SocialChallengePublishingController(SocialChallengePublishingService publishingService)
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
