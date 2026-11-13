using Games.Domain.Challenges;
using Shared.Domain;
using Shared.Domain.Exceptions;

namespace Games.Domain.Quizzes;

public sealed class Quiz : AggregateRoot
{
    private const int MaxNameLength = 200;
    private const int MinQuestions = 1;
    private const int MaxQuestions = 20;

    private readonly List<Question> _questions = [];

    public string Name { get; private set; }
    public string Instructions { get; private set; }
    public int Xp { get; private set; }
    public ChallengeStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public IReadOnlyList<Question> Questions => _questions;

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
        Status = ChallengeStatus.Draft;
        CreatedAt = DateTime.UtcNow;
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

    public void Delete()
    {
        if (Status != ChallengeStatus.Draft)
        {
            throw new DomainException("Only a draft quiz can be deleted.");
        }
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
