using Games.Application.QuizAttempts;
using Games.Domain.QuizAttempts;

namespace Games.Infrastructure.Persistence.QuizAttempts;

internal sealed class QuizAttemptRepository : IQuizAttemptRepository
{
    private readonly GamesDbContext _dbContext;

    public QuizAttemptRepository(GamesDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public void Add(QuizAttempt attempt)
    {
        _dbContext.QuizAttempts.Add(attempt);
    }
}
