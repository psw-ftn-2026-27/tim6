using Games.Application.Quizzes;

namespace Games.Application.QuizAuthoring;

public sealed class QuizAuthoringQueries
{
    private readonly IQuizReadRepository _quizReadRepository;

    public QuizAuthoringQueries(IQuizReadRepository quizReadRepository)
    {
        _quizReadRepository = quizReadRepository;
    }

    public Task<List<QuizDto>> GetAllAsync()
    {
        return _quizReadRepository.GetAllAsync();
    }
}
