using Games.Application.QuizAuthoring;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Games.Api;

[ApiController]
[Route("api/games/quizzes")]
[Authorize(Roles = "administrator")]
public sealed class QuizAuthoringController : ControllerBase
{
    private readonly QuizAuthoringService _authoringService;
    private readonly QuizAuthoringQueries _authoringQueries;

    public QuizAuthoringController(QuizAuthoringService authoringService, QuizAuthoringQueries authoringQueries)
    {
        _authoringService = authoringService;
        _authoringQueries = authoringQueries;
    }

    [HttpGet]
    public async Task<ActionResult<List<QuizDto>>> GetAll()
    {
        return await _authoringQueries.GetAllAsync();
    }

    [HttpPost]
    public async Task<ActionResult<Guid>> Create(CreateQuizDto dto)
    {
        return await _authoringService.CreateAsync(dto);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult> Edit(Guid id, UpdateQuizDto dto)
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
