using System.Net;
using System.Net.Http.Json;
using Exploration.Application.TourPreferenceEditing;
using Exploration.Domain.Tours;
using Exploration.Infrastructure.Persistence;
using Exploration.Tests.Integration.Seeds;
using FluentAssertions;
using Shared.Tests;
using Xunit;

namespace Exploration.Tests.Integration.TourPreferenceEditing;

public class TourPreferenceEditingCommandTests : BaseIntegrationTest
{
    public TourPreferenceEditingCommandTests(ExplorationApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Tourist_saves_preferences_for_the_first_time()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");
        var request = new TourPreferenceDto([TourDifficulty.Hard], TransportMode.Bicycle, ["planina"], true);
        using var arrangeContext = Factory.CreateContext<ExplorationDbContext>();
        var preferenceCountBefore = arrangeContext.TourPreferences.Count();

        var response = await client.PutAsJsonAsync("/api/exploration/tour-preferences/mine", request);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        assertContext.TourPreferences.Count().Should().Be(preferenceCountBefore + 1);
        var stored = assertContext.TourPreferences.Single(preference => preference.TouristId == WellKnownUsers.SecondExplorer);
        stored.Difficulties.Should().Equal(TourDifficulty.Hard);
        stored.Transport.Should().Be(TransportMode.Bicycle);
        stored.Tags.Should().Equal("planina");
        stored.ApplyToSearch.Should().BeTrue();
    }

    [Fact]
    public async Task Saving_again_changes_the_existing_preference()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var request = new TourPreferenceDto([TourDifficulty.Hard], TransportMode.Car, [], false);
        using var arrangeContext = Factory.CreateContext<ExplorationDbContext>();
        var preferenceCountBefore = arrangeContext.TourPreferences.Count();

        var response = await client.PutAsJsonAsync("/api/exploration/tour-preferences/mine", request);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        assertContext.TourPreferences.Count().Should().Be(preferenceCountBefore);
        var stored = assertContext.TourPreferences.Single(preference => preference.TouristId == WellKnownUsers.Explorer);
        stored.Id.Should().Be(TourPreferenceSeed.ExplorersPreference.Id);
        stored.Difficulties.Should().Equal(TourDifficulty.Hard);
        stored.Transport.Should().Be(TransportMode.Car);
        stored.Tags.Should().BeEmpty();
        stored.ApplyToSearch.Should().BeFalse();
    }

    [Fact]
    public async Task Tourist_cannot_save_preferences_without_a_difficulty()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");
        var request = new TourPreferenceDto([], TransportMode.Walking, ["grad"], false);
        using var arrangeContext = Factory.CreateContext<ExplorationDbContext>();
        var preferenceCountBefore = arrangeContext.TourPreferences.Count();

        var response = await client.PutAsJsonAsync("/api/exploration/tour-preferences/mine", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        assertContext.TourPreferences.Count().Should().Be(preferenceCountBefore);
        assertContext.TourPreferences.Should().NotContain(preference => preference.TouristId == WellKnownUsers.SecondExplorer);
    }

    [Fact]
    public async Task Anonymous_user_cannot_save_preferences()
    {
        var client = Factory.CreateClient();
        var request = new TourPreferenceDto([TourDifficulty.Easy], TransportMode.Walking, [], false);

        var response = await client.PutAsJsonAsync("/api/exploration/tour-preferences/mine", request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Tourist_deletes_their_preference()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        using var arrangeContext = Factory.CreateContext<ExplorationDbContext>();
        var preferenceCountBefore = arrangeContext.TourPreferences.Count();

        var response = await client.DeleteAsync("/api/exploration/tour-preferences/mine");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        assertContext.TourPreferences.Count().Should().Be(preferenceCountBefore - 1);
        assertContext.TourPreferences.Should().NotContain(preference => preference.Id == TourPreferenceSeed.ExplorersPreference.Id);
    }

    [Fact]
    public async Task Tourist_cannot_delete_a_preference_that_does_not_exist()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");
        using var arrangeContext = Factory.CreateContext<ExplorationDbContext>();
        var preferenceCountBefore = arrangeContext.TourPreferences.Count();

        var response = await client.DeleteAsync("/api/exploration/tour-preferences/mine");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        assertContext.TourPreferences.Count().Should().Be(preferenceCountBefore);
    }

    [Fact]
    public async Task Anonymous_user_cannot_delete_preferences()
    {
        var client = Factory.CreateClient();

        var response = await client.DeleteAsync("/api/exploration/tour-preferences/mine");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
