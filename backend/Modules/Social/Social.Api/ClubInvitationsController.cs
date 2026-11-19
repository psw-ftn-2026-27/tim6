using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Api;
using Social.Application.ClubInvitations;

namespace Social.Api;

[ApiController]
[Route("api/social/clubs")]
[Authorize]
public sealed class ClubInvitationsController : ControllerBase
{
    private readonly ClubInvitationsService _invitationsService;
    private readonly ClubInvitationsQueries _invitationsQueries;

    public ClubInvitationsController(
        ClubInvitationsService invitationsService,
        ClubInvitationsQueries invitationsQueries)
    {
        _invitationsService = invitationsService;
        _invitationsQueries = invitationsQueries;
    }

    [HttpGet("{id:guid}/invitations")]
    public async Task<ActionResult<List<ClubInvitationDto>>> GetInvitations(Guid id)
    {
        return await _invitationsQueries.GetInvitationsAsync(id, User.GetUserId());
    }

    [HttpPost("{id:guid}/invitations")]
    public async Task<ActionResult> Invite(Guid id, InviteDto dto)
    {
        await _invitationsService.InviteAsync(id, User.GetUserId(), dto);
        return NoContent();
    }

    [HttpDelete("{id:guid}/invitations/{inviteeId:guid}")]
    public async Task<ActionResult> WithdrawInvitation(Guid id, Guid inviteeId)
    {
        await _invitationsService.WithdrawInvitationAsync(id, User.GetUserId(), inviteeId);
        return NoContent();
    }
}
