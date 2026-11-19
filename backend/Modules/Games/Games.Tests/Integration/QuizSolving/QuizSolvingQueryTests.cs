using System.Net;
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

    [Fact]
    public async Task Quiz_opened_from_too_far_away_is_refused()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var quiz = QuizSeed.PublishedFortressGates;

        var response = await client.GetAsync(FormattableString.Invariant(
            $"/api/games/quizzes/{quiz.Id}/solving?latitude={quiz.Location!.Latitude + 0.01}&longitude={quiz.Location.Longitude}"));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Quiz_that_does_not_exist_cannot_be_opened()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.GetAsync(
            $"/api/games/quizzes/{Guid.NewGuid()}/solving?latitude=45.2520&longitude=19.8610");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Quiz_the_explorer_already_solved_cannot_be_opened_again()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var quiz = QuizSeed.PublishedMuseumStreet;

        var response = await client.GetAsync(FormattableString.Invariant(
            $"/api/games/quizzes/{quiz.Id}/solving?latitude={quiz.Location!.Latitude}&longitude={quiz.Location.Longitude}"));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Explorer_cannot_see_the_result_of_another_explorers_attempt()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");

        var response = await client.GetAsync(
            $"/api/games/quizzes/attempts/{QuizAttemptSeed.ExplorerHalfCorrectOnFortressGates.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task List_is_ordered_from_the_nearest_quiz_to_the_farthest()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var explorerLocation = new Location(45.2520, 19.8610);

        var quizzes = await client.GetFromJsonAsync<List<PublishedQuizDto>>(FormattableString.Invariant(
            $"/api/games/quizzes/published?latitude={explorerLocation.Latitude}&longitude={explorerLocation.Longitude}"),
            JsonOptions);

        var expectedOrder = QuizSeed.All
            .Where(quiz => quiz.Status == ChallengeStatus.Published)
            .OrderBy(quiz => quiz.Location!.DistanceTo(explorerLocation))
            .Select(quiz => quiz.Id);
        quizzes!.Select(quiz => quiz.Id).Should().Equal(expectedOrder);
    }
}
