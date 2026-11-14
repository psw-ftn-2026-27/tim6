using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Api;
using Social.Application.Following;

namespace Social.Api;

[ApiController]
[Route("api/social")]
[Authorize]
public sealed class FollowingController : ControllerBase
{
    private readonly FollowingService _followingService;
    private readonly FollowingQueries _followingQueries;

    public FollowingController(FollowingService followingService, FollowingQueries followingQueries)
    {
        _followingService = followingService;
        _followingQueries = followingQueries;
    }

    [HttpPost("users/{recipientId:guid}/follow-requests")]
    public async Task<ActionResult<Guid>> SendFollowRequest(Guid recipientId)
    {
        return await _followingService.SendFollowRequestAsync(User.GetUserId(), recipientId);
    }

    [HttpDelete("follow-requests/{requestId:guid}")]
    public async Task<ActionResult> CancelFollowRequest(Guid requestId)
    {
        await _followingService.CancelFollowRequestAsync(User.GetUserId(), requestId);
        return NoContent();
    }

    [HttpDelete("following/{followedUserId:guid}")]
    public async Task<ActionResult> UnfollowUser(Guid followedUserId)
    {
        await _followingService.UnfollowUserAsync(User.GetUserId(), followedUserId);
        return NoContent();
    }

    [HttpGet("users/{otherUserId:guid}/follow-status")]
    public async Task<ActionResult<FollowStatusDto>> GetFollowStatus(Guid otherUserId)
    {
        return await _followingQueries.GetFollowStatusAsync(User.GetUserId(), otherUserId);
    }

    [HttpGet("follow-requests/sent")]
    public async Task<ActionResult<List<SentFollowRequestDto>>> GetSentFollowRequests()
    {
        return await _followingQueries.GetSentRequestsAsync(User.GetUserId());
    }

    [HttpGet("following")]
    public async Task<ActionResult<List<FollowedUserDto>>> GetFollowedUsers()
    {
        return await _followingQueries.GetFollowedUsersAsync(User.GetUserId());
    }

    [HttpPost("follow-requests/{requestId:guid}/accept")]
    public async Task<ActionResult> AcceptFollowRequest(Guid requestId)
    {
        await _followingService.AcceptFollowRequestAsync(User.GetUserId(), requestId);
        return NoContent();
    }

    [HttpPost("follow-requests/{requestId:guid}/reject")]
    public async Task<ActionResult> RejectFollowRequest(Guid requestId)
    {
        await _followingService.RejectFollowRequestAsync(User.GetUserId(), requestId);
        return NoContent();
    }

    [HttpDelete("followers/{followerId:guid}")]
    public async Task<ActionResult> RemoveFollower(Guid followerId)
    {
        await _followingService.RemoveFollowerAsync(User.GetUserId(), followerId);
        return NoContent();
    }

    [HttpGet("follow-requests/received")]
    public async Task<ActionResult<List<ReceivedFollowRequestDto>>> GetReceivedFollowRequests()
    {
        return await _followingQueries.GetReceivedRequestsAsync(User.GetUserId());
    }

    [HttpGet("followers")]
    public async Task<ActionResult<List<FollowerDto>>> GetFollowers()
    {
        return await _followingQueries.GetFollowersAsync(User.GetUserId());
    }
}
