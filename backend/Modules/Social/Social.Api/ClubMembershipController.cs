using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Api;
using Social.Application.ClubMembership;

namespace Social.Api;

[ApiController]
[Route("api/social/clubs")]
[Authorize]
public sealed class ClubMembershipController : ControllerBase
{
    private readonly ClubMembershipService _membershipService;
    private readonly ClubMembershipQueries _membershipQueries;

    public ClubMembershipController(
        ClubMembershipService membershipService,
        ClubMembershipQueries membershipQueries)
    {
        _membershipService = membershipService;
        _membershipQueries = membershipQueries;
    }

    [HttpGet("{id:guid}/membership")]
    public async Task<ActionResult<ClubMembershipDto>> GetMembership(Guid id)
    {
        return await _membershipQueries.GetMembershipAsync(id, User.GetUserId());
    }

    [HttpPost("{id:guid}/join-request")]
    public async Task<ActionResult> RequestToJoin(Guid id)
    {
        await _membershipService.RequestToJoinAsync(id, User.GetUserId());
        return NoContent();
    }

    [HttpDelete("{id:guid}/join-request")]
    public async Task<ActionResult> WithdrawJoinRequest(Guid id)
    {
        await _membershipService.WithdrawJoinRequestAsync(id, User.GetUserId());
        return NoContent();
    }
}
