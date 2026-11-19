namespace Games.Application.QuizSolving;

// EarnedXp is everything the tourist has earned on the quiz so far, out of the quiz's QuizXp;
// AwardedXp is the part this attempt added.
public sealed record QuizAttemptResultDto(
    int CorrectAnswers,
    int QuestionCount,
    int AwardedXp,
    int EarnedXp,
    int QuizXp,
    bool SolvedQuiz);
