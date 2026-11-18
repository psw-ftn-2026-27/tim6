using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Api;
using Social.Application.ClubModeration;

namespace Social.Api;

[ApiController]
[Route("api/social/clubs")]
[Authorize]
public sealed class ClubModerationController : ControllerBase
{
    private readonly ClubModerationService _moderationService;
    private readonly ClubModerationQueries _moderationQueries;

    public ClubModerationController(
        ClubModerationService moderationService,
        ClubModerationQueries moderationQueries)
    {
        _moderationService = moderationService;
        _moderationQueries = moderationQueries;
    }

    [HttpGet("{id:guid}/join-requests")]
    public async Task<ActionResult<List<ClubJoinRequestDto>>> GetJoinRequests(Guid id)
    {
        return await _moderationQueries.GetJoinRequestsAsync(id, User.GetUserId());
    }

    [HttpPost("{id:guid}/join-requests/{applicantId:guid}/accept")]
    public async Task<ActionResult> AcceptJoinRequest(Guid id, Guid applicantId)
    {
        await _moderationService.AcceptJoinRequestAsync(id, User.GetUserId(), applicantId);
        return NoContent();
    }

    [HttpPost("{id:guid}/join-requests/{applicantId:guid}/reject")]
    public async Task<ActionResult> RejectJoinRequest(Guid id, Guid applicantId)
    {
        await _moderationService.RejectJoinRequestAsync(id, User.GetUserId(), applicantId);
        return NoContent();
    }
}
