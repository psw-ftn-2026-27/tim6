namespace Games.Application.QuizAuthoring;

public sealed record QuestionDto(string Text, List<AnswerOptionDto> Options);
