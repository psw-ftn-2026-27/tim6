using Games.Application.Challenges;

namespace Games.Application.QuizAuthoring;

public sealed record CreateQuizDto(string Name, string Instructions, int Xp, List<QuestionDto> Questions,
    LocationDto? Location);
