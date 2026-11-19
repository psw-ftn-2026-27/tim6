using Games.Application.Challenges;
using Games.Application.QuizSolving;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Api;

namespace Games.Api;

[ApiController]
[Route("api/games/quizzes")]
[Authorize(Roles = "explorer")]
public sealed class QuizSolvingController : ControllerBase
{
    private readonly QuizSolvingService _solvingService;
    private readonly QuizSolvingQueries _solvingQueries;

    public QuizSolvingController(QuizSolvingService solvingService, QuizSolvingQueries solvingQueries)
    {
        _solvingService = solvingService;
        _solvingQueries = solvingQueries;
    }

    [HttpGet("{id:guid}/solving")]
    public async Task<ActionResult<QuizForSolvingDto>> GetForSolving(Guid id, [FromQuery] LocationDto position)
    {
        return await _solvingQueries.GetQuizForSolvingAsync(id, User.GetUserId(), position);
    }

    [HttpPost("{id:guid}/attempts")]
    public async Task<ActionResult<Guid>> SubmitAttempt(Guid id, SubmitAttemptDto dto)
    {
        return await _solvingService.SubmitAttemptAsync(id, User.GetUserId(), dto);
    }

    [HttpGet("attempts/{attemptId:guid}")]
    public async Task<ActionResult<QuizAttemptResultDto>> GetAttemptResult(Guid attemptId)
    {
        return await _solvingQueries.GetAttemptResultAsync(attemptId, User.GetUserId());
    }
}
