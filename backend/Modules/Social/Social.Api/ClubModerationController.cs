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

    [HttpGet("{id:guid}/members")]
    public async Task<ActionResult<List<ClubMemberDto>>> GetMembers(Guid id)
    {
        return await _moderationQueries.GetMembersAsync(id, User.GetUserId());
    }

    [HttpPost("{id:guid}/members/{memberId:guid}/promote")]
    public async Task<ActionResult> PromoteToModerator(Guid id, Guid memberId)
    {
        await _moderationService.PromoteToModeratorAsync(id, User.GetUserId(), memberId);
        return NoContent();
    }

    [HttpPost("{id:guid}/members/{memberId:guid}/transfer-ownership")]
    public async Task<ActionResult> TransferOwnership(Guid id, Guid memberId)
    {
        await _moderationService.TransferOwnershipAsync(id, User.GetUserId(), memberId);
        return NoContent();
    }

    [HttpDelete("{id:guid}/members/{memberId:guid}")]
    public async Task<ActionResult> RemoveMember(Guid id, Guid memberId)
    {
        await _moderationService.RemoveMemberAsync(id, User.GetUserId(), memberId);
        return NoContent();
    }
}
