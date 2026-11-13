using Games.Application.QuizAuthoring;

namespace Games.Application.Quizzes;

public interface IQuizReadRepository
{
    Task<List<QuizDto>> GetAllAsync();
}
