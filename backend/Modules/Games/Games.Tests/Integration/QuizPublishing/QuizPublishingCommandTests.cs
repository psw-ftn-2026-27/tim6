using System.Net;
using FluentAssertions;
using Games.Domain.Challenges;
using Games.Infrastructure.Persistence;
using Games.Tests.Integration.Seeds;
using Shared.Tests;
using Xunit;

namespace Games.Tests.Integration.QuizPublishing;

public class QuizPublishingCommandTests : BaseIntegrationTest
{
    public QuizPublishingCommandTests(GamesApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Administrator_publishes_a_draft_with_a_location()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.PostAsync($"/api/games/quizzes/{QuizSeed.LocatedDraftDanubeBridges.Id}/publish", null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        var stored = assertContext.Quizzes.Single(quiz => quiz.Id == QuizSeed.LocatedDraftDanubeBridges.Id);
        stored.Status.Should().Be(ChallengeStatus.Published);
    }

    [Fact]
    public async Task Draft_without_a_location_cannot_be_published_and_stays_a_draft()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.PostAsync($"/api/games/quizzes/{QuizSeed.DraftFortressHistory.Id}/publish", null);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        var stored = assertContext.Quizzes.Single(quiz => quiz.Id == QuizSeed.DraftFortressHistory.Id);
        stored.Status.Should().Be(ChallengeStatus.Draft);
    }

    [Fact]
    public async Task Unknown_quiz_cannot_be_published()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        using var arrangeContext = Factory.CreateContext<GamesDbContext>();
        var publishedCountBefore = arrangeContext.Quizzes.Count(quiz => quiz.Status == ChallengeStatus.Published);

        var response = await client.PostAsync($"/api/games/quizzes/{Guid.NewGuid()}/publish", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        assertContext.Quizzes.Count(quiz => quiz.Status == ChallengeStatus.Published).Should().Be(publishedCountBefore);
    }

    [Fact]
    public async Task Anonymous_user_cannot_publish_a_quiz()
    {
        var client = Factory.CreateClient();

        var response = await client.PostAsync($"/api/games/quizzes/{QuizSeed.LocatedDraftDanubeBridges.Id}/publish", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Explorer_cannot_publish_a_quiz()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.PostAsync($"/api/games/quizzes/{QuizSeed.LocatedDraftDanubeBridges.Id}/publish", null);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Administrator_retires_a_published_quiz()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.PostAsync($"/api/games/quizzes/{QuizSeed.PublishedOldTown.Id}/retire", null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        var stored = assertContext.Quizzes.Single(quiz => quiz.Id == QuizSeed.PublishedOldTown.Id);
        stored.Status.Should().Be(ChallengeStatus.Retired);
    }

    [Fact]
    public async Task Draft_cannot_be_retired_and_stays_a_draft()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.PostAsync($"/api/games/quizzes/{QuizSeed.LocatedDraftDanubeBridges.Id}/retire", null);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        var stored = assertContext.Quizzes.Single(quiz => quiz.Id == QuizSeed.LocatedDraftDanubeBridges.Id);
        stored.Status.Should().Be(ChallengeStatus.Draft);
    }

    [Fact]
    public async Task Unknown_quiz_cannot_be_retired()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        using var arrangeContext = Factory.CreateContext<GamesDbContext>();
        var retiredCountBefore = arrangeContext.Quizzes.Count(quiz => quiz.Status == ChallengeStatus.Retired);

        var response = await client.PostAsync($"/api/games/quizzes/{Guid.NewGuid()}/retire", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        assertContext.Quizzes.Count(quiz => quiz.Status == ChallengeStatus.Retired).Should().Be(retiredCountBefore);
    }

    [Fact]
    public async Task Anonymous_user_cannot_retire_a_quiz()
    {
        var client = Factory.CreateClient();

        var response = await client.PostAsync($"/api/games/quizzes/{QuizSeed.PublishedOldTown.Id}/retire", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Explorer_cannot_retire_a_quiz()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.PostAsync($"/api/games/quizzes/{QuizSeed.PublishedOldTown.Id}/retire", null);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Administrator_archives_a_retired_quiz()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.PostAsync($"/api/games/quizzes/{QuizSeed.RetiredRiversideParks.Id}/archive", null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        var stored = assertContext.Quizzes.Single(quiz => quiz.Id == QuizSeed.RetiredRiversideParks.Id);
        stored.Status.Should().Be(ChallengeStatus.Archived);
    }

    [Fact]
    public async Task Published_quiz_cannot_be_archived_and_stays_published()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.PostAsync($"/api/games/quizzes/{QuizSeed.PublishedOldTown.Id}/archive", null);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        var stored = assertContext.Quizzes.Single(quiz => quiz.Id == QuizSeed.PublishedOldTown.Id);
        stored.Status.Should().Be(ChallengeStatus.Published);
    }

    [Fact]
    public async Task Unknown_quiz_cannot_be_archived()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        using var arrangeContext = Factory.CreateContext<GamesDbContext>();
        var archivedCountBefore = arrangeContext.Quizzes.Count(quiz => quiz.Status == ChallengeStatus.Archived);

        var response = await client.PostAsync($"/api/games/quizzes/{Guid.NewGuid()}/archive", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        assertContext.Quizzes.Count(quiz => quiz.Status == ChallengeStatus.Archived).Should().Be(archivedCountBefore);
    }

    [Fact]
    public async Task Anonymous_user_cannot_archive_a_quiz()
    {
        var client = Factory.CreateClient();

        var response = await client.PostAsync($"/api/games/quizzes/{QuizSeed.RetiredRiversideParks.Id}/archive", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Explorer_cannot_archive_a_quiz()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.PostAsync($"/api/games/quizzes/{QuizSeed.RetiredRiversideParks.Id}/archive", null);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
