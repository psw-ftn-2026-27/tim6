using Games.Domain.QuizAttempts;

namespace Games.Application.QuizAttempts;

public interface IQuizAttemptRepository
{
    void Add(QuizAttempt attempt);
}
