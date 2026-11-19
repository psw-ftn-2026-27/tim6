namespace Games.Application.QuizSolving;

// A question without the mark of its correct answer option.
public sealed record QuestionForSolvingDto(string Text, List<string> Options);
