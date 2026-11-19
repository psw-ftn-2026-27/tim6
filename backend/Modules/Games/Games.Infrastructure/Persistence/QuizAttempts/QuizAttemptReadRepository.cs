using Games.Application.QuizAttempts;
using Games.Application.QuizSolving;
using Microsoft.EntityFrameworkCore;

namespace Games.Infrastructure.Persistence.QuizAttempts;

internal sealed class QuizAttemptReadRepository : IQuizAttemptReadRepository
{
    private readonly GamesDbContext _dbContext;

    public QuizAttemptReadRepository(GamesDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<QuizAttemptResultDto?> GetResultAsync(Guid attemptId, Guid touristId)
    {
        return _dbContext.QuizAttempts
            .Where(attempt => attempt.Id == attemptId && attempt.TouristId == touristId)
            .Select(attempt => new QuizAttemptResultDto(
                attempt.CorrectAnswers,
                attempt.QuestionCount,
                attempt.AwardedXp,
                _dbContext.ChallengeProgresses
                    .Where(progress => progress.ChallengeId == attempt.QuizId && progress.TouristId == touristId)
                    .Select(progress => progress.EarnedXp)
                    .FirstOrDefault(),
                _dbContext.Quizzes
                    .Where(quiz => quiz.Id == attempt.QuizId)
                    .Select(quiz => quiz.Xp)
                    .FirstOrDefault(),
                attempt.SolvedQuiz))
            .FirstOrDefaultAsync();
    }
}
