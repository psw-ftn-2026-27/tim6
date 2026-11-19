using Games.Application.Challenges;
using Games.Application.QuizAuthoring;
using Games.Application.QuizSolving;
using Games.Application.Quizzes;
using Games.Domain.Challenges;
using Microsoft.EntityFrameworkCore;

namespace Games.Infrastructure.Persistence.Quizzes;

internal sealed class QuizReadRepository : IQuizReadRepository
{
    private readonly GamesDbContext _dbContext;

    public QuizReadRepository(GamesDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<QuizDto>> GetAllAsync()
    {
        // Nested JSON collections lose their order when projected in a query over many rows,
        // so the questions are read as stored and mapped to DTOs after the query.
        var quizzes = await _dbContext.Quizzes
            .AsNoTracking()
            .OrderByDescending(quiz => quiz.CreatedAt)
            .ThenBy(quiz => quiz.Id)
            .Select(quiz => new { quiz.Id, quiz.Name, quiz.Instructions, quiz.Xp, quiz.Status, quiz.Questions, quiz.Location })
            .ToListAsync();

        return quizzes
            .Select(quiz => new QuizDto(
                quiz.Id,
                quiz.Name,
                quiz.Instructions,
                quiz.Xp,
                quiz.Status,
                quiz.Questions
                    .Select(question => new QuestionDto(
                        question.Text,
                        question.Options
                            .Select(option => new AnswerOptionDto(option.Text, option.IsCorrect))
                            .ToList()))
                    .ToList(),
                quiz.Location is null ? null : new LocationDto(quiz.Location.Latitude, quiz.Location.Longitude)))
            .ToList();
    }

    public Task<List<PublishedQuizLocationDto>> GetPublishedAsync(Guid touristId)
    {
        return _dbContext.Quizzes
            .Where(quiz => quiz.Status == ChallengeStatus.Published)
            .Select(quiz => new PublishedQuizLocationDto(
                quiz.Id,
                quiz.Name,
                quiz.Xp,
                new LocationDto(quiz.Location!.Latitude, quiz.Location.Longitude),
                _dbContext.ChallengeProgresses.Any(progress =>
                    progress.ChallengeId == quiz.Id && progress.TouristId == touristId && progress.SolvedAt != null)))
            .ToListAsync();
    }
}
