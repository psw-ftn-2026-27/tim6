using Shared.Domain.Exceptions;

namespace Games.Domain.Quizzes;

public sealed record AnswerOption
{
    public string Text { get; private init; }
    public bool IsCorrect { get; private init; }

    private AnswerOption()
    {
        Text = null!;
    }

    public AnswerOption(string text, bool isCorrect)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            throw new DomainException("An answer option requires a text.");
        }

        Text = text;
        IsCorrect = isCorrect;
    }
}
