using Games.Application.QuizPublishing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Games.Api;

[ApiController]
[Route("api/games/quizzes")]
[Authorize(Roles = "administrator")]
public sealed class QuizPublishingController : ControllerBase
{
    private readonly QuizPublishingService _publishingService;

    public QuizPublishingController(QuizPublishingService publishingService)
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
