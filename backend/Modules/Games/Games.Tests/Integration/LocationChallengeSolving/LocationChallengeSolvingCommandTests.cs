using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Games.Application.Challenges;
using Games.Domain.LocationChallenges;
using Games.Infrastructure.Persistence;
using Games.Tests.Integration.Seeds;
using Shared.Tests;
using Xunit;

namespace Games.Tests.Integration.LocationChallengeSolving;

public class LocationChallengeSolvingCommandTests : BaseIntegrationTest
{
    public LocationChallengeSolvingCommandTests(GamesApiFactory factory) : base(factory) { }

    private static LocationDto AtStartOf(LocationChallenge challenge) =>
        new(challenge.Location!.Latitude, challenge.Location.Longitude);

    private static readonly LocationDto FarAway = new(45.2700, 19.8300);

    [Fact]
    public async Task Explorer_near_the_start_location_activates_a_challenge_and_gets_an_attempt_with_a_radius_in_the_allowed_range()
    {
        var challenge = LocationChallengeSeed.PublishedRiversideDetails;
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        using var arrangeContext = Factory.CreateContext<GamesDbContext>();
        var attemptCountBefore = arrangeContext.LocationChallengeAttempts.Count();
        var farthestPointDistance = challenge.HiddenPoints.Max(point => challenge.Location!.DistanceTo(point.Location));

        var response = await client.PostAsJsonAsync($"/api/games/location-challenges/{challenge.Id}/activate", AtStartOf(challenge));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var attemptId = await response.Content.ReadFromJsonAsync<Guid>(JsonOptions);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        assertContext.LocationChallengeAttempts.Count().Should().Be(attemptCountBefore + 1);
        var stored = assertContext.LocationChallengeAttempts.Single(attempt => attempt.Id == attemptId);
        stored.LocationChallengeId.Should().Be(challenge.Id);
        stored.ExplorerId.Should().Be(WellKnownUsers.Explorer);
        stored.RadiusInMeters.Should().BeInRange(farthestPointDistance, farthestPointDistance + 100);
        stored.CompletedAt.Should().BeNull();
        stored.RevealedHiddenPointIds.Should().BeEmpty();
    }

    [Fact]
    public async Task Activating_an_open_attempt_again_continues_it_and_the_explorer_still_has_one_attempt_for_the_challenge()
    {
        var openAttempt = LocationChallengeAttemptSeed.ExplorersOpenBridgeViewsWithFirstPointRevealed;
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.PostAsJsonAsync($"/api/games/location-challenges/{openAttempt.LocationChallengeId}/activate", FarAway);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var attemptId = await response.Content.ReadFromJsonAsync<Guid>(JsonOptions);
        attemptId.Should().Be(openAttempt.Id);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        var stored = assertContext.LocationChallengeAttempts.Single(attempt =>
            attempt.LocationChallengeId == openAttempt.LocationChallengeId && attempt.ExplorerId == WellKnownUsers.Explorer);
        stored.Id.Should().Be(openAttempt.Id);
        stored.RadiusInMeters.Should().Be(openAttempt.RadiusInMeters);
        stored.RevealedHiddenPointIds.Should().Equal(openAttempt.RevealedHiddenPointIds);
    }

    [Fact]
    public async Task Activation_too_far_from_the_start_location_stores_no_attempt()
    {
        var challenge = LocationChallengeSeed.PublishedRiversideDetails;
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        using var arrangeContext = Factory.CreateContext<GamesDbContext>();
        var attemptCountBefore = arrangeContext.LocationChallengeAttempts.Count();

        var response = await client.PostAsJsonAsync($"/api/games/location-challenges/{challenge.Id}/activate", FarAway);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        assertContext.LocationChallengeAttempts.Count().Should().Be(attemptCountBefore);
        assertContext.LocationChallengeAttempts.Any(attempt => attempt.LocationChallengeId == challenge.Id).Should().BeFalse();
    }

    [Fact]
    public async Task Unknown_location_challenge_cannot_be_activated()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        using var arrangeContext = Factory.CreateContext<GamesDbContext>();
        var attemptCountBefore = arrangeContext.LocationChallengeAttempts.Count();

        var response = await client.PostAsJsonAsync($"/api/games/location-challenges/{Guid.NewGuid()}/activate", FarAway);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        assertContext.LocationChallengeAttempts.Count().Should().Be(attemptCountBefore);
    }

    [Fact]
    public async Task Anonymous_user_cannot_activate_a_location_challenge()
    {
        var challenge = LocationChallengeSeed.PublishedRiversideDetails;
        var client = Factory.CreateClient();

        var response = await client.PostAsJsonAsync($"/api/games/location-challenges/{challenge.Id}/activate", AtStartOf(challenge));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_activate_a_location_challenge()
    {
        var challenge = LocationChallengeSeed.PublishedRiversideDetails;
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.PostAsJsonAsync($"/api/games/location-challenges/{challenge.Id}/activate", AtStartOf(challenge));

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Check_at_the_last_unrevealed_hidden_point_stores_it_and_completes_the_attempt()
    {
        var challenge = LocationChallengeSeed.PublishedBridgeViews;
        var lastPoint = challenge.HiddenPoints[1];
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var position = new LocationDto(lastPoint.Location.Latitude, lastPoint.Location.Longitude);

        var response = await client.PostAsJsonAsync($"/api/games/location-challenges/{challenge.Id}/check-location", position);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        var stored = assertContext.LocationChallengeAttempts.Single(attempt =>
            attempt.Id == LocationChallengeAttemptSeed.ExplorersOpenBridgeViewsWithFirstPointRevealed.Id);
        stored.RevealedHiddenPointIds.Should().BeEquivalentTo(challenge.HiddenPoints.Select(point => point.Id));
        stored.CompletedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task Completing_a_location_challenge_records_the_explorers_progress_as_solved_with_the_challenge_xp()
    {
        var challenge = LocationChallengeSeed.PublishedBridgeViews;
        var lastPoint = challenge.HiddenPoints[1];
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var position = new LocationDto(lastPoint.Location.Latitude, lastPoint.Location.Longitude);

        var response = await client.PostAsJsonAsync($"/api/games/location-challenges/{challenge.Id}/check-location", position);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        var progress = assertContext.ChallengeProgresses.Single(stored =>
            stored.ChallengeId == challenge.Id && stored.TouristId == WellKnownUsers.Explorer);
        progress.EarnedXp.Should().Be(challenge.Xp);
        progress.IsSolved().Should().BeTrue();
    }

    [Fact]
    public async Task Check_far_from_every_hidden_point_leaves_the_attempt_unchanged()
    {
        var openAttempt = LocationChallengeAttemptSeed.ExplorersOpenBridgeViewsWithFirstPointRevealed;
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.PostAsJsonAsync($"/api/games/location-challenges/{openAttempt.LocationChallengeId}/check-location", FarAway);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        var stored = assertContext.LocationChallengeAttempts.Single(attempt => attempt.Id == openAttempt.Id);
        stored.RevealedHiddenPointIds.Should().Equal(openAttempt.RevealedHiddenPointIds);
        stored.CompletedAt.Should().BeNull();
        assertContext.ChallengeProgresses.Any(progress => progress.ChallengeId == openAttempt.LocationChallengeId).Should().BeFalse();
    }

    [Fact]
    public async Task Check_without_an_open_attempt_is_rejected()
    {
        var challenge = LocationChallengeSeed.PublishedBridgeViews;
        var firstPoint = challenge.HiddenPoints[0];
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");
        var position = new LocationDto(firstPoint.Location.Latitude, firstPoint.Location.Longitude);

        var response = await client.PostAsJsonAsync($"/api/games/location-challenges/{challenge.Id}/check-location", position);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        assertContext.LocationChallengeAttempts.Any(attempt => attempt.ExplorerId == WellKnownUsers.SecondExplorer).Should().BeFalse();
    }

    [Fact]
    public async Task Anonymous_user_cannot_check_a_location()
    {
        var challenge = LocationChallengeSeed.PublishedBridgeViews;
        var client = Factory.CreateClient();

        var response = await client.PostAsJsonAsync($"/api/games/location-challenges/{challenge.Id}/check-location", FarAway);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_check_a_location()
    {
        var challenge = LocationChallengeSeed.PublishedBridgeViews;
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.PostAsJsonAsync($"/api/games/location-challenges/{challenge.Id}/check-location", FarAway);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
