using Games.Domain.Challenges;
using Shared.Domain.Exceptions;

namespace Games.Domain.Quizzes;

public sealed class Quiz : Challenge
{
    private const int MaxNameLength = 200;
    private const int MinQuestions = 1;
    private const int MaxQuestions = 20;

    private readonly List<Question> _questions = [];

    public string Name { get; private set; }
    public string Instructions { get; private set; }
    public int Xp { get; private set; }
    public IReadOnlyList<Question> Questions => _questions;

    // Every quiz is activated within the same radius, so a read use case can show it without loading a quiz.
    public const int ActivationRadius = 50;

    protected override string KindName => "quiz";
    public override int ActivationRadiusInMeters => ActivationRadius;

    private Quiz()
    {
        Name = null!;
        Instructions = null!;
    }

    public Quiz(string name, string instructions, int xp, List<Question> questions)
        : base(Guid.NewGuid())
    {
        Validate(name, instructions, xp, questions);

        Name = name;
        Instructions = instructions;
        Xp = xp;
        _questions.AddRange(questions);
    }

    public void Edit(string name, string instructions, int xp, List<Question> questions)
    {
        if (Status != ChallengeStatus.Draft)
        {
            throw new DomainException("Only a draft quiz can be edited.");
        }
        Validate(name, instructions, xp, questions);

        Name = name;
        Instructions = instructions;
        Xp = xp;
        _questions.Clear();
        _questions.AddRange(questions);
    }

    public QuizScore Grade(List<SelectedAnswer> answers)
    {
        if (answers is null || answers.Count != _questions.Count)
        {
            throw new DomainException("An attempt answers every question of the quiz.");
        }
        if (answers.Select(answer => answer.QuestionIndex).Distinct().Count() != answers.Count)
        {
            throw new DomainException("An attempt answers each question exactly once.");
        }
        foreach (var answer in answers)
        {
            if (answer.QuestionIndex < 0 || answer.QuestionIndex >= _questions.Count)
            {
                throw new DomainException($"The quiz has no question {answer.QuestionIndex}.");
            }
            if (answer.OptionIndex < 0 || answer.OptionIndex >= _questions[answer.QuestionIndex].Options.Count)
            {
                throw new DomainException($"Question {answer.QuestionIndex} has no answer option {answer.OptionIndex}.");
            }
        }

        var correctAnswers = answers.Count(answer => _questions[answer.QuestionIndex].Options[answer.OptionIndex].IsCorrect);
        return new QuizScore(correctAnswers, Xp * correctAnswers / _questions.Count, correctAnswers == _questions.Count);
    }

    private static void Validate(string name, string instructions, int xp, List<Question> questions)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("A quiz requires a name.");
        }
        if (name.Length > MaxNameLength)
        {
            throw new DomainException($"A quiz name has at most {MaxNameLength} characters.");
        }
        if (string.IsNullOrWhiteSpace(instructions))
        {
            throw new DomainException("A quiz requires instructions.");
        }
        if (xp <= 0)
        {
            throw new DomainException("A quiz rewards a positive amount of XP.");
        }
        if (questions is null || questions.Count < MinQuestions || questions.Count > MaxQuestions)
        {
            throw new DomainException($"A quiz has between {MinQuestions} and {MaxQuestions} questions.");
        }
    }
}
