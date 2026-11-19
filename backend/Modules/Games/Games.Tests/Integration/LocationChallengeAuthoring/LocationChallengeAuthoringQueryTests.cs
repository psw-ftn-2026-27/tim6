using System.Net.Http.Json;
using FluentAssertions;
using Games.Application.Challenges;
using Games.Application.LocationChallengeAuthoring;
using Games.Tests.Integration.Seeds;
using Shared.Tests;
using Xunit;

namespace Games.Tests.Integration.LocationChallengeAuthoring;

public class LocationChallengeAuthoringQueryTests : BaseIntegrationTest
{
    public LocationChallengeAuthoringQueryTests(GamesApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Administrator_sees_every_location_challenge_with_its_status_start_location_and_hidden_point_locations()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var challenges = await client.GetFromJsonAsync<List<LocationChallengeDto>>("/api/games/location-challenges", JsonOptions);

        var expected = LocationChallengeSeed.All.Select(challenge => new LocationChallengeDto(
            challenge.Id,
            challenge.Name,
            challenge.Xp,
            challenge.Status,
            challenge.HiddenPoints
                .Select(point => new HiddenPointDto(point.Image, new LocationDto(point.Location.Latitude, point.Location.Longitude)))
                .ToList(),
            challenge.Location is null ? null : new LocationDto(challenge.Location.Latitude, challenge.Location.Longitude)));
        challenges.Should().BeEquivalentTo(expected, options => options.ComparingRecordsByMembers());
    }
}
