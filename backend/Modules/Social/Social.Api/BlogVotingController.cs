using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Api;
using Social.Application.BlogVoting;

namespace Social.Api;

[ApiController]
[Route("api/social/blogs")]
[Authorize]
public sealed class BlogVotingController : ControllerBase
{
    private readonly BlogVotingService _votingService;
    private readonly BlogVotingQueries _votingQueries;

    public BlogVotingController(BlogVotingService votingService, BlogVotingQueries votingQueries)
    {
        _votingService = votingService;
        _votingQueries = votingQueries;
    }

    [HttpPost("{id:guid}/votes")]
    public async Task<ActionResult> CastVote(Guid id, CastVoteDto dto)
    {
        await _votingService.CastVoteAsync(id, User.GetUserId(), dto);
        return NoContent();
    }

    [HttpGet("{id:guid}/votes/mine")]
    public async Task<ActionResult<MyVoteDto>> GetMyVote(Guid id)
    {
        return await _votingQueries.GetMyVoteAsync(id, User.GetUserId());
    }
}
