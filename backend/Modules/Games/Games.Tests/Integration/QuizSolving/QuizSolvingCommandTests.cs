using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Games.Application.Challenges;
using Games.Application.QuizSolving;
using Games.Infrastructure.Persistence;
using Games.Tests.Integration.Seeds;
using Shared.Tests;
using Xunit;

namespace Games.Tests.Integration.QuizSolving;

public class QuizSolvingCommandTests : BaseIntegrationTest
{
    private static readonly LocationDto AtFortressGates = new(45.2520, 19.8610);
    private static readonly LocationDto FarFromFortressGates = new(45.2620, 19.8610);
    private static readonly LocationDto AtMuseumStreet = new(45.2560, 19.8470);
    private static readonly LocationDto AtOldTown = new(45.2551, 19.8451);

    private static readonly List<SelectedAnswerDto> AllCorrectFortressGatesAnswers = [new(0, 0), new(1, 0)];

    public QuizSolvingCommandTests(GamesApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Nearby_explorer_submits_answers_and_the_attempt_and_the_progress_with_the_awarded_xp_are_stored()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var quizId = QuizSeed.PublishedFortressGates.Id;
        using var arrangeContext = Factory.CreateContext<GamesDbContext>();
        var attemptCountBefore = arrangeContext.QuizAttempts.Count();

        var response = await client.PostAsJsonAsync($"/api/games/quizzes/{quizId}/attempts",
            new SubmitAttemptDto(AtFortressGates, AllCorrectFortressGatesAnswers), JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var attemptId = await response.Content.ReadFromJsonAsync<Guid>(JsonOptions);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        assertContext.QuizAttempts.Count().Should().Be(attemptCountBefore + 1);
        var attempt = assertContext.QuizAttempts.Single(stored => stored.Id == attemptId);
        attempt.QuizId.Should().Be(quizId);
        attempt.TouristId.Should().Be(WellKnownUsers.Explorer);
        attempt.Answers.Select(answer => new SelectedAnswerDto(answer.QuestionIndex, answer.OptionIndex))
            .Should().Equal(AllCorrectFortressGatesAnswers);
        attempt.AwardedXp.Should().Be(50);
        attempt.SolvedQuiz.Should().BeTrue();
        var progress = assertContext.ChallengeProgresses.Single(stored =>
            stored.Id == ChallengeProgressSeed.ExplorerHalfwayThroughFortressGates.Id);
        progress.EarnedXp.Should().Be(QuizSeed.PublishedFortressGates.Xp);
        progress.SolvedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task First_attempt_on_a_quiz_starts_the_explorers_progress_on_it()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var quiz = QuizSeed.PublishedOldTown;

        var response = await client.PostAsJsonAsync($"/api/games/quizzes/{quiz.Id}/attempts",
            new SubmitAttemptDto(AtOldTown, [new SelectedAnswerDto(0, 0)]), JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        var progress = assertContext.ChallengeProgresses.Single(stored =>
            stored.ChallengeId == quiz.Id && stored.TouristId == WellKnownUsers.Explorer);
        progress.EarnedXp.Should().Be(quiz.Xp);
        progress.SolvedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task Answers_for_a_quiz_that_does_not_exist_are_rejected()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        using var arrangeContext = Factory.CreateContext<GamesDbContext>();
        var attemptCountBefore = arrangeContext.QuizAttempts.Count();

        var response = await client.PostAsJsonAsync($"/api/games/quizzes/{Guid.NewGuid()}/attempts",
            new SubmitAttemptDto(AtFortressGates, AllCorrectFortressGatesAnswers), JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        assertContext.QuizAttempts.Count().Should().Be(attemptCountBefore);
    }

    [Fact]
    public async Task Answers_sent_from_too_far_away_are_rejected_and_no_attempt_is_stored()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        using var arrangeContext = Factory.CreateContext<GamesDbContext>();
        var attemptCountBefore = arrangeContext.QuizAttempts.Count();

        var response = await client.PostAsJsonAsync($"/api/games/quizzes/{QuizSeed.PublishedFortressGates.Id}/attempts",
            new SubmitAttemptDto(FarFromFortressGates, AllCorrectFortressGatesAnswers), JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        assertContext.QuizAttempts.Count().Should().Be(attemptCountBefore);
    }

    [Fact]
    public async Task Answers_for_an_already_solved_quiz_are_rejected_and_no_attempt_is_stored()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        using var arrangeContext = Factory.CreateContext<GamesDbContext>();
        var attemptCountBefore = arrangeContext.QuizAttempts.Count();

        var response = await client.PostAsJsonAsync($"/api/games/quizzes/{QuizSeed.PublishedMuseumStreet.Id}/attempts",
            new SubmitAttemptDto(AtMuseumStreet, [new SelectedAnswerDto(0, 0)]), JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        assertContext.QuizAttempts.Count().Should().Be(attemptCountBefore);
    }

    [Fact]
    public async Task Anonymous_user_cannot_submit_answers()
    {
        var client = Factory.CreateClient();

        var response = await client.PostAsJsonAsync($"/api/games/quizzes/{QuizSeed.PublishedFortressGates.Id}/attempts",
            new SubmitAttemptDto(AtFortressGates, AllCorrectFortressGatesAnswers), JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_submit_answers()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.PostAsJsonAsync($"/api/games/quizzes/{QuizSeed.PublishedFortressGates.Id}/attempts",
            new SubmitAttemptDto(AtFortressGates, AllCorrectFortressGatesAnswers), JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
