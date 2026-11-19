using Games.Application.ChallengeMap;
using Games.Application.QuizAuthoring;

namespace Games.Application.Quizzes;

public interface IQuizReadRepository
{
    Task<List<QuizDto>> GetAllAsync();
    Task<List<QuizOnMapDto>> GetPublishedForMapAsync(Guid touristId);
}
