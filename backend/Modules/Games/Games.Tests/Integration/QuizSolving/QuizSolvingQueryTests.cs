using System.Net.Http.Json;
using FluentAssertions;
using Games.Application.QuizSolving;
using Games.Domain.Challenges;
using Games.Tests.Integration.Seeds;
using Shared.Tests;
using Xunit;

namespace Games.Tests.Integration.QuizSolving;

public class QuizSolvingQueryTests : BaseIntegrationTest
{
    public QuizSolvingQueryTests(GamesApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Quiz_for_solving_arrives_without_the_marks_of_correct_answers()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var quiz = QuizSeed.PublishedFortressGates;

        var response = await client.GetAsync(FormattableString.Invariant(
            $"/api/games/quizzes/{quiz.Id}/solving?latitude={quiz.Location!.Latitude}&longitude={quiz.Location.Longitude}"));

        var body = await response.Content.ReadAsStringAsync();
        body.Should().NotContainEquivalentOf("isCorrect");
        var solving = await response.Content.ReadFromJsonAsync<QuizForSolvingDto>(JsonOptions);
        var expected = new QuizForSolvingDto(
            quiz.Id,
            quiz.Name,
            quiz.Instructions,
            quiz.Questions
                .Select(question => new QuestionForSolvingDto(
                    question.Text,
                    question.Options.Select(option => option.Text).ToList()))
                .ToList());
        solving.Should().BeEquivalentTo(expected, options => options.ComparingRecordsByMembers().WithStrictOrdering());
    }

    [Fact]
    public async Task List_holds_only_published_quizzes_and_marks_those_the_explorer_solved()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var quizzes = await client.GetFromJsonAsync<List<PublishedQuizDto>>(
            "/api/games/quizzes/published?latitude=45.2551&longitude=19.8451", JsonOptions);

        var expected = QuizSeed.All
            .Where(quiz => quiz.Status == ChallengeStatus.Published)
            .Select(quiz => new
            {
                quiz.Id,
                IsSolved = ChallengeProgressSeed.All.Any(progress =>
                    progress.ChallengeId == quiz.Id && progress.TouristId == WellKnownUsers.Explorer && progress.IsSolved())
            });
        quizzes.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task Attempt_result_shows_the_xp_earned_on_the_quiz_out_of_the_quiz_xp()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var attempt = QuizAttemptSeed.ExplorerHalfCorrectOnFortressGates;

        var result = await client.GetFromJsonAsync<QuizAttemptResultDto>(
            $"/api/games/quizzes/attempts/{attempt.Id}", JsonOptions);

        var expected = new QuizAttemptResultDto(
            attempt.CorrectAnswers,
            attempt.QuestionCount,
            attempt.AwardedXp,
            ChallengeProgressSeed.ExplorerHalfwayThroughFortressGates.EarnedXp,
            QuizSeed.PublishedFortressGates.Xp,
            attempt.SolvedQuiz);
        result.Should().Be(expected);
    }
}
