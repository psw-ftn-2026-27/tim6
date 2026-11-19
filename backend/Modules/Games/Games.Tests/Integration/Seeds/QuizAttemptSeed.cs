using Games.Domain.QuizAttempts;
using Games.Domain.Quizzes;
using Shared.Tests;

namespace Games.Tests.Integration.Seeds;

internal static class QuizAttemptSeed
{
    public static readonly QuizAttempt ExplorerHalfCorrectOnFortressGates;

    static QuizAttemptSeed()
    {
        List<SelectedAnswer> halfCorrectAnswers = [new SelectedAnswer(0, 0), new SelectedAnswer(1, 1)];
        var halfCorrectScore = QuizSeed.PublishedFortressGates.Grade(halfCorrectAnswers);
        ExplorerHalfCorrectOnFortressGates = new(QuizSeed.PublishedFortressGates.Id, WellKnownUsers.Explorer,
            halfCorrectAnswers, halfCorrectScore, 50, new DateTime(2026, 11, 1, 10, 0, 0, DateTimeKind.Utc));
    }

    public static QuizAttempt[] All => [ExplorerHalfCorrectOnFortressGates];
}
