namespace Games.Application.QuizAuthoring;

public sealed record UpdateQuizDto(string Name, string Instructions, int Xp, List<QuestionDto> Questions);
