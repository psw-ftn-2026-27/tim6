using Games.Application.Challenges;

namespace Games.Application.QuizSolving;

public sealed record SubmitAttemptDto(LocationDto Position, List<SelectedAnswerDto> Answers);
