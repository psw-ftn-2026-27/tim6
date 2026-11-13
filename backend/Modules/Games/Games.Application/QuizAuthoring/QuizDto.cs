using Games.Domain.Challenges;

namespace Games.Application.QuizAuthoring;

public sealed record QuizDto(
    Guid Id,
    string Name,
    string Instructions,
    int Xp,
    ChallengeStatus Status,
    List<QuestionDto> Questions);
