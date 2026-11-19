using Games.Application.QuizAuthoring;
using Games.Application.QuizSolving;

namespace Games.Application.Quizzes;

public interface IQuizReadRepository
{
    Task<List<QuizDto>> GetAllAsync();
    Task<List<PublishedQuizLocationDto>> GetPublishedAsync(Guid touristId);
}
