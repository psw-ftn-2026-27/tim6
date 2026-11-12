using Games.Application.SocialChallengeAuthoring;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Games.Api;

[ApiController]
[Route("api/games/social-challenges")]
[Authorize(Roles = "administrator")]
public sealed class SocialChallengeAuthoringController : ControllerBase
{
    private readonly SocialChallengeAuthoringService _authoringService;
    private readonly SocialChallengeAuthoringQueries _authoringQueries;

    public SocialChallengeAuthoringController(
        SocialChallengeAuthoringService authoringService,
        SocialChallengeAuthoringQueries authoringQueries)
    {
        _authoringService = authoringService;
        _authoringQueries = authoringQueries;
    }

    [HttpGet]
    public async Task<ActionResult<List<SocialChallengeDto>>> GetAll()
    {
        return await _authoringQueries.GetAllAsync();
    }

    [HttpPost]
    public async Task<ActionResult<SocialChallengeDto>> Create(CreateSocialChallengeDto dto)
    {
        return await _authoringService.CreateAsync(dto);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult> Edit(Guid id, UpdateSocialChallengeDto dto)
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
