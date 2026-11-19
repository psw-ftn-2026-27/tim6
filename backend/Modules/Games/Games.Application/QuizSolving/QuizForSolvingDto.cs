namespace Games.Application.QuizSolving;

public sealed record QuizForSolvingDto(Guid Id, string Name, string Instructions, List<QuestionForSolvingDto> Questions);
