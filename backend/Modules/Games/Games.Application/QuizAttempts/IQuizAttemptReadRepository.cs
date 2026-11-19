using Games.Application.QuizSolving;

namespace Games.Application.QuizAttempts;

public interface IQuizAttemptReadRepository
{
    Task<QuizAttemptResultDto?> GetResultAsync(Guid attemptId, Guid touristId);
}
