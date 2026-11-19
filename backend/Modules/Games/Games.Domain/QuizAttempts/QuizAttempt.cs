using Games.Domain.Quizzes;
using Shared.Domain;

namespace Games.Domain.QuizAttempts;

// One finished attempt at a quiz: every answer the tourist gave and what the attempt earned.
public sealed class QuizAttempt : AggregateRoot
{
    private readonly List<SelectedAnswer> _answers = [];

    public Guid QuizId { get; private set; }
    public Guid TouristId { get; private set; }
    public IReadOnlyList<SelectedAnswer> Answers => _answers;
    public int CorrectAnswers { get; private set; }
    public int QuestionCount { get; private set; }
    public int AwardedXp { get; private set; }
    public bool SolvedQuiz { get; private set; }
    public DateTime FinishedAt { get; private set; }

    private QuizAttempt() { }

    public QuizAttempt(Guid quizId, Guid touristId, List<SelectedAnswer> answers, QuizScore score, int awardedXp,
        DateTime finishedAt)
        : base(Guid.NewGuid())
    {
        QuizId = quizId;
        TouristId = touristId;
        _answers.AddRange(answers);
        CorrectAnswers = score.CorrectAnswers;
        QuestionCount = answers.Count;
        AwardedXp = awardedXp;
        SolvedQuiz = score.AllCorrect;
        FinishedAt = finishedAt;
    }
}
