using System.Net.Http.Json;
using FluentAssertions;
using Games.Application.Challenges;
using Games.Application.LocationChallengeSolving;
using Games.Domain.Challenges;
using Games.Tests.Integration.Seeds;
using Shared.Domain;
using Shared.Tests;
using Xunit;

namespace Games.Tests.Integration.LocationChallengeSolving;

public class LocationChallengeSolvingQueryTests : BaseIntegrationTest
{
    public LocationChallengeSolvingQueryTests(GamesApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Explorer_sees_published_challenges_they_have_not_solved_with_attempts_in_progress_marked()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var challenges = await client.GetFromJsonAsync<PageResult<AvailableLocationChallengeDto>>(
            "/api/games/location-challenges/available?pageSize=100", JsonOptions);

        var explorersAttempts = LocationChallengeAttemptSeed.All.Where(attempt => attempt.ExplorerId == WellKnownUsers.Explorer).ToList();
        var expected = LocationChallengeSeed.All
            .Where(challenge => challenge.Status == ChallengeStatus.Published)
            .Where(challenge => !explorersAttempts.Any(attempt => attempt.LocationChallengeId == challenge.Id && attempt.CompletedAt is not null))
            .Select(challenge => new AvailableLocationChallengeDto(
                challenge.Id,
                challenge.Name,
                new LocationDto(challenge.Location!.Latitude, challenge.Location.Longitude),
                challenge.Xp,
                explorersAttempts.Any(attempt => attempt.LocationChallengeId == challenge.Id && attempt.CompletedAt is null)));
        challenges!.Items.Should().BeEquivalentTo(expected, options => options.ComparingRecordsByMembers());
        challenges.Items.Should().Contain(challenge => challenge.Id == LocationChallengeSeed.PublishedBridgeViews.Id && challenge.HasAttemptInProgress);
        challenges.Items.Should().NotContain(challenge => challenge.Id == LocationChallengeSeed.PublishedSquareStatues.Id);
        challenges.Items.Should().NotContain(challenge => challenge.Id == LocationChallengeSeed.RetiredParkSculptures.Id);
    }

    [Fact]
    public async Task Attempt_shows_the_circle_and_every_photo_but_only_the_locations_of_revealed_hidden_points()
    {
        var challenge = LocationChallengeSeed.PublishedBridgeViews;
        var openAttempt = LocationChallengeAttemptSeed.ExplorersOpenBridgeViewsWithFirstPointRevealed;
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var attempt = await client.GetFromJsonAsync<LocationChallengeAttemptDto>(
            $"/api/games/location-challenges/{challenge.Id}/attempt", JsonOptions);

        var expected = new LocationChallengeAttemptDto(
            openAttempt.Id,
            new LocationDto(challenge.Location!.Latitude, challenge.Location.Longitude),
            openAttempt.RadiusInMeters,
            challenge.HiddenPoints
                .Select(point => openAttempt.RevealedHiddenPointIds.Contains(point.Id)
                    ? new AttemptHiddenPointDto(point.Id, point.Image, true, new LocationDto(point.Location.Latitude, point.Location.Longitude))
                    : new AttemptHiddenPointDto(point.Id, point.Image, false, null))
                .ToList(),
            null,
            null);
        attempt.Should().BeEquivalentTo(expected, options => options.ComparingRecordsByMembers());
        attempt!.HiddenPoints.Should().Contain(point => point.Location == null);
    }
}
