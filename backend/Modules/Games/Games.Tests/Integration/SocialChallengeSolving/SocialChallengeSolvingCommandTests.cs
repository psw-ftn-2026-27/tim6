using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Games.Application.Challenges;
using Games.Domain.SocialChallenges;
using Games.Infrastructure.Persistence;
using Games.Tests.Integration.Seeds;
using Shared.Tests;
using Xunit;

namespace Games.Tests.Integration.SocialChallengeSolving;

public class SocialChallengeSolvingCommandTests : BaseIntegrationTest
{
    private static readonly LocationDto AtRiversidePicnic = new(45.2551, 19.8547);

    public SocialChallengeSolvingCommandTests(GamesApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Activation_of_a_social_challenge_the_explorer_already_solved_is_rejected_and_no_run_starts()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        using var arrangeContext = Factory.CreateContext<GamesDbContext>();
        var runCountBefore = arrangeContext.Set<SocialChallengeRun>().Count();

        var response = await client.PostAsJsonAsync(
            $"/api/games/social-challenges/{SocialChallengeSeed.PublishedRiversidePicnic.Id}/activate",
            AtRiversidePicnic, JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        assertContext.Set<SocialChallengeRun>().Count().Should().Be(runCountBefore);
    }

    [Fact]
    public async Task Activation_of_a_social_challenge_that_does_not_exist_is_rejected_and_no_run_starts()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");
        using var arrangeContext = Factory.CreateContext<GamesDbContext>();
        var runCountBefore = arrangeContext.Set<SocialChallengeRun>().Count();

        var response = await client.PostAsJsonAsync($"/api/games/social-challenges/{Guid.NewGuid()}/activate",
            AtRiversidePicnic, JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        assertContext.Set<SocialChallengeRun>().Count().Should().Be(runCountBefore);
    }

    [Fact]
    public async Task Anonymous_user_cannot_activate_a_social_challenge()
    {
        var client = Factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            $"/api/games/social-challenges/{SocialChallengeSeed.PublishedRiversidePicnic.Id}/activate",
            AtRiversidePicnic, JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_activate_a_social_challenge()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.PostAsJsonAsync(
            $"/api/games/social-challenges/{SocialChallengeSeed.PublishedRiversidePicnic.Id}/activate",
            AtRiversidePicnic, JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Position_check_for_a_social_challenge_that_does_not_exist_is_rejected_and_no_progress_is_stored()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");
        using var arrangeContext = Factory.CreateContext<GamesDbContext>();
        var progressCountBefore = arrangeContext.ChallengeProgresses.Count();

        var response = await client.PostAsJsonAsync($"/api/games/social-challenges/{Guid.NewGuid()}/check-position",
            AtRiversidePicnic, JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        assertContext.ChallengeProgresses.Count().Should().Be(progressCountBefore);
    }

    [Fact]
    public async Task Anonymous_user_cannot_check_a_position_in_a_social_challenge()
    {
        var client = Factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            $"/api/games/social-challenges/{SocialChallengeSeed.PublishedRiversidePicnic.Id}/check-position",
            AtRiversidePicnic, JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_check_a_position_in_a_social_challenge()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.PostAsJsonAsync(
            $"/api/games/social-challenges/{SocialChallengeSeed.PublishedRiversidePicnic.Id}/check-position",
            AtRiversidePicnic, JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
