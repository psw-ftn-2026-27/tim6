namespace Games.Domain.Quizzes;

public sealed record QuizScore
{
    public int CorrectAnswers { get; }
    public int Xp { get; }
    public bool AllCorrect { get; }

    public QuizScore(int correctAnswers, int xp, bool allCorrect)
    {
        CorrectAnswers = correctAnswers;
        Xp = xp;
        AllCorrect = allCorrect;
    }
}
