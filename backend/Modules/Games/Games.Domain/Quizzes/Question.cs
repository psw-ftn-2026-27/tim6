using Shared.Domain.Exceptions;

namespace Games.Domain.Quizzes;

public sealed record Question
{
    private const int MinOptions = 2;
    private const int MaxOptions = 6;

    private readonly List<AnswerOption> _options = [];

    public string Text { get; private init; }
    public IReadOnlyList<AnswerOption> Options => _options;

    private Question()
    {
        Text = null!;
    }

    public Question(string text, List<AnswerOption> options)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            throw new DomainException("A question requires a text.");
        }
        if (options is null || options.Count < MinOptions || options.Count > MaxOptions)
        {
            throw new DomainException($"A question has between {MinOptions} and {MaxOptions} answer options.");
        }
        var distinctTexts = options.Select(option => option.Text.Trim().ToLowerInvariant()).Distinct().Count();
        if (distinctTexts != options.Count)
        {
            throw new DomainException("The answer options of a question do not repeat.");
        }
        if (options.Count(option => option.IsCorrect) != 1)
        {
            throw new DomainException("A question has exactly one correct answer option.");
        }

        Text = text;
        _options.AddRange(options);
    }
}
