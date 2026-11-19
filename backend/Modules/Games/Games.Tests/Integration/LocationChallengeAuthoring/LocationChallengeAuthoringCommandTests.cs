using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Games.Application.Challenges;
using Games.Application.LocationChallengeAuthoring;
using Games.Domain.Challenges;
using Games.Domain.LocationChallenges;
using Games.Infrastructure.Persistence;
using Games.Tests.Integration.Seeds;
using Shared.Tests;
using Xunit;

namespace Games.Tests.Integration.LocationChallengeAuthoring;

public class LocationChallengeAuthoringCommandTests : BaseIntegrationTest
{
    public LocationChallengeAuthoringCommandTests(GamesApiFactory factory) : base(factory) { }

    private static List<HiddenPointDto> CreateHiddenPoints() =>
    [
        new HiddenPointDto("/api/shared/images/most-1.jpg", new LocationDto(45.2580, 19.8612)),
        new HiddenPointDto("/api/shared/images/most-2.jpg", new LocationDto(45.2594, 19.8577)),
        new HiddenPointDto("/api/shared/images/most-3.jpg", new LocationDto(45.2602, 19.8540))
    ];

    private static List<HiddenPointDto> ToHiddenPointDtos(IEnumerable<HiddenPoint> hiddenPoints) =>
        hiddenPoints
            .Select(point => new HiddenPointDto(point.Image, new LocationDto(point.Location.Latitude, point.Location.Longitude)))
            .ToList();

    [Fact]
    public async Task Administrator_creates_a_draft_stored_with_its_start_location_and_all_hidden_points()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var request = new CreateLocationChallengeDto("Mostovi izbliza", 130, CreateHiddenPoints(), new LocationDto(45.2575, 19.8620));
        using var arrangeContext = Factory.CreateContext<GamesDbContext>();
        var challengeCountBefore = arrangeContext.LocationChallenges.Count();

        var response = await client.PostAsJsonAsync("/api/games/location-challenges", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var createdId = await response.Content.ReadFromJsonAsync<Guid>(JsonOptions);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        assertContext.LocationChallenges.Count().Should().Be(challengeCountBefore + 1);
        var stored = assertContext.LocationChallenges.Single(challenge => challenge.Id == createdId);
        stored.Name.Should().Be("Mostovi izbliza");
        stored.Xp.Should().Be(130);
        stored.Status.Should().Be(ChallengeStatus.Draft);
        stored.Location.Should().Be(new Location(45.2575, 19.8620));
        ToHiddenPointDtos(stored.HiddenPoints).Should().BeEquivalentTo(request.HiddenPoints,
            options => options.ComparingRecordsByMembers());
    }

    [Fact]
    public async Task Location_challenge_without_hidden_points_is_not_stored()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var request = new CreateLocationChallengeDto("Mostovi izbliza", 130, [], null);
        using var arrangeContext = Factory.CreateContext<GamesDbContext>();
        var challengeCountBefore = arrangeContext.LocationChallenges.Count();

        var response = await client.PostAsJsonAsync("/api/games/location-challenges", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        assertContext.LocationChallenges.Count().Should().Be(challengeCountBefore);
    }

    [Fact]
    public async Task Anonymous_user_cannot_create_a_location_challenge()
    {
        var client = Factory.CreateClient();
        var request = new CreateLocationChallengeDto("Mostovi izbliza", 130, CreateHiddenPoints(), null);

        var response = await client.PostAsJsonAsync("/api/games/location-challenges", request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Explorer_cannot_create_a_location_challenge()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var request = new CreateLocationChallengeDto("Mostovi izbliza", 130, CreateHiddenPoints(), null);

        var response = await client.PostAsJsonAsync("/api/games/location-challenges", request);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Edited_draft_is_stored_with_new_data_and_its_old_hidden_points_are_replaced()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var request = new UpdateLocationChallengeDto("Mostovi izbliza", 130, CreateHiddenPoints(), new LocationDto(45.2575, 19.8620));
        var oldHiddenPointIds = LocationChallengeSeed.LocatedDraftFortressCorners.HiddenPoints.Select(point => point.Id).ToList();

        var response = await client.PutAsJsonAsync($"/api/games/location-challenges/{LocationChallengeSeed.LocatedDraftFortressCorners.Id}", request);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        var stored = assertContext.LocationChallenges.Single(challenge => challenge.Id == LocationChallengeSeed.LocatedDraftFortressCorners.Id);
        stored.Name.Should().Be("Mostovi izbliza");
        stored.Xp.Should().Be(130);
        stored.Status.Should().Be(ChallengeStatus.Draft);
        stored.Location.Should().Be(new Location(45.2575, 19.8620));
        ToHiddenPointDtos(stored.HiddenPoints).Should().BeEquivalentTo(request.HiddenPoints,
            options => options.ComparingRecordsByMembers());
        assertContext.Set<HiddenPoint>().Any(point => oldHiddenPointIds.Contains(point.Id)).Should().BeFalse();
    }

    [Fact]
    public async Task Published_location_challenge_is_not_edited_and_keeps_its_data()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var request = new UpdateLocationChallengeDto("Mostovi izbliza", 130, CreateHiddenPoints(), null);

        var response = await client.PutAsJsonAsync($"/api/games/location-challenges/{LocationChallengeSeed.PublishedRiversideDetails.Id}", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        var stored = assertContext.LocationChallenges.Single(challenge => challenge.Id == LocationChallengeSeed.PublishedRiversideDetails.Id);
        stored.Name.Should().Be(LocationChallengeSeed.PublishedRiversideDetails.Name);
        stored.Xp.Should().Be(LocationChallengeSeed.PublishedRiversideDetails.Xp);
        stored.Status.Should().Be(ChallengeStatus.Published);
        stored.HiddenPoints.Select(point => point.Id).Should()
            .BeEquivalentTo(LocationChallengeSeed.PublishedRiversideDetails.HiddenPoints.Select(point => point.Id));
    }

    [Fact]
    public async Task Unknown_location_challenge_cannot_be_edited()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var request = new UpdateLocationChallengeDto("Mostovi izbliza", 130, CreateHiddenPoints(), null);

        var response = await client.PutAsJsonAsync($"/api/games/location-challenges/{Guid.NewGuid()}", request);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        assertContext.LocationChallenges.Any(challenge => challenge.Name == "Mostovi izbliza").Should().BeFalse();
    }

    [Fact]
    public async Task Anonymous_user_cannot_edit_a_location_challenge()
    {
        var client = Factory.CreateClient();
        var request = new UpdateLocationChallengeDto("Mostovi izbliza", 130, CreateHiddenPoints(), null);

        var response = await client.PutAsJsonAsync($"/api/games/location-challenges/{LocationChallengeSeed.DraftHiddenFacades.Id}", request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Explorer_cannot_edit_a_location_challenge()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var request = new UpdateLocationChallengeDto("Mostovi izbliza", 130, CreateHiddenPoints(), null);

        var response = await client.PutAsJsonAsync($"/api/games/location-challenges/{LocationChallengeSeed.DraftHiddenFacades.Id}", request);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Deleted_draft_no_longer_exists_together_with_its_hidden_points()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var hiddenPointIds = LocationChallengeSeed.DraftHiddenFacades.HiddenPoints.Select(point => point.Id).ToList();
        using var arrangeContext = Factory.CreateContext<GamesDbContext>();
        var challengeCountBefore = arrangeContext.LocationChallenges.Count();

        var response = await client.DeleteAsync($"/api/games/location-challenges/{LocationChallengeSeed.DraftHiddenFacades.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        assertContext.LocationChallenges.Count().Should().Be(challengeCountBefore - 1);
        assertContext.LocationChallenges.Any(challenge => challenge.Id == LocationChallengeSeed.DraftHiddenFacades.Id).Should().BeFalse();
        assertContext.Set<HiddenPoint>().Any(point => hiddenPointIds.Contains(point.Id)).Should().BeFalse();
    }

    [Fact]
    public async Task Unknown_location_challenge_cannot_be_deleted()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        using var arrangeContext = Factory.CreateContext<GamesDbContext>();
        var challengeCountBefore = arrangeContext.LocationChallenges.Count();

        var response = await client.DeleteAsync($"/api/games/location-challenges/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        assertContext.LocationChallenges.Count().Should().Be(challengeCountBefore);
    }

    [Fact]
    public async Task Anonymous_user_cannot_delete_a_location_challenge()
    {
        var client = Factory.CreateClient();

        var response = await client.DeleteAsync($"/api/games/location-challenges/{LocationChallengeSeed.DraftHiddenFacades.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Explorer_cannot_delete_a_location_challenge()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.DeleteAsync($"/api/games/location-challenges/{LocationChallengeSeed.DraftHiddenFacades.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
