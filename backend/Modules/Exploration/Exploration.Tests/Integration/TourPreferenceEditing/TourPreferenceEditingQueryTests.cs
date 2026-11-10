using System.Net;
using System.Net.Http.Json;
using Exploration.Application.TourPreferenceEditing;
using Exploration.Tests.Integration.Seeds;
using FluentAssertions;
using Shared.Tests;
using Xunit;

namespace Exploration.Tests.Integration.TourPreferenceEditing;

public class TourPreferenceEditingQueryTests : BaseIntegrationTest
{
    public TourPreferenceEditingQueryTests(ExplorationApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Tourist_sees_their_saved_preference()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var preference = await client.GetFromJsonAsync<TourPreferenceDto>("/api/exploration/tour-preferences/mine", JsonOptions);

        preference!.Difficulties.Should().Equal(TourPreferenceSeed.ExplorersPreference.Difficulties);
        preference.Transport.Should().Be(TourPreferenceSeed.ExplorersPreference.Transport);
        preference.Tags.Should().Equal(TourPreferenceSeed.ExplorersPreference.Tags);
    }

    [Fact]
    public async Task Tourist_does_not_see_another_tourists_preference()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");

        var response = await client.GetAsync("/api/exploration/tour-preferences/mine");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
}
